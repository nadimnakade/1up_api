/**
 * Document360 Multi-Workspace Portal — external custom script
 *
 * Setup (external JS/CSS only — no HTML/template access needed):
 *   Document360 Admin → Settings → Custom Scripts
 *     JS:  https://1up.co.in/NEXUS_API/Scripts/doc360-multiworkspace.js
 *     CSS (optional — the JS auto-injects it if missing):
 *          https://1up.co.in/NEXUS_API/Content/doc360-multiworkspace.css
 *
 * Modes (auto-detected from URL, re-detected on SPA navigation):
 *   - PORTAL (no /docs/ in path): full-page dashboard, cards navigate to
 *     real Document360 articles. No JWT → login banner; public cards stay.
 *   - KB (/docs/ in path): floating "All Workspaces" button → slide-out sidebar.
 *
 * Flow: ?token= capture → localStorage → Bearer call to NEXUS_API →
 * API validates JWT and returns authorized workspace content only.
 */
(function () {
    'use strict';

    if (window.__doc360MwLoaded) return;
    window.__doc360MwLoaded = true;

    // ========================= CONFIG =========================
    var CONFIG = {
        API_BASE: 'https://1up.co.in/NEXUS_API',
        // Login page for the portal banner (your productLogin page)
        LOGIN_URL: 'https://1up.co.in/NEXUS_API/Nomadix/login.html',
        // Candidate article URLs across workspaces — edit to match your portal
        URLS: [
            'https://kms.cloud.global/trustedwifi/docs/getting-started',
            'https://kms.cloud.global/trustedwifi/docs/administration',
            'https://kms.cloud.global/nomadix/docs/installation',
            'https://kms.cloud.global/nomadix/docs/configuration'
        ],
        TOKEN_KEY: 'doc360_jwt',
        PANEL_ID: 'doc360-mw-panel',
        BTN_ID: 'doc360-mw-btn'
    };

    var lastPath = location.pathname;
    var currentMode = null;
    var portalTimer = null;
    var portalTries = 0;
    var portalCache = null;
    var loadedOnce = false;

    // ===================== TOKEN CAPTURE ======================
    function captureToken() {
        try {
            var m = location.search.match(/[?&]token=([^&]+)/);
            if (m) {
                var token = decodeURIComponent(m[1]);
                localStorage.setItem(CONFIG.TOKEN_KEY, token);
                var cleaned = location.search
                    .replace(/[?&]token=[^&]+/, '')
                    .replace(/^\?$/, '')
                    .replace(/^\?&/, '?');
                history.replaceState(null, '', location.pathname + cleaned + location.hash);
            }
        } catch (e) { /* storage may be blocked */ }
    }

    function getToken() {
        try { return localStorage.getItem(CONFIG.TOKEN_KEY); } catch (e) { return null; }
    }

    function clearToken() {
        try { localStorage.removeItem(CONFIG.TOKEN_KEY); } catch (e) {}
    }

    // ======================== MODE ============================
    function detectMode() {
        // Article pages contain /docs/; portal home does not
        return location.pathname.indexOf('/docs/') === -1 ? 'portal' : 'kb';
    }

    // ======================== FETCH ===========================
    function fetchWorkspaces(urls) {
        var token = getToken();
        if (!token) {
            return Promise.reject({ status: 401, message: 'Not logged in.' });
        }

        return fetch(CONFIG.API_BASE + '/api/Document360/GetMultiWorkspaceContent', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            },
            body: JSON.stringify({ Urls: urls })
        }).then(function (response) {
            if (response.status === 401) {
                clearToken();
                portalCache = null;
                return response.text().then(function (t) {
                    throw { status: 401, message: 'Session expired. Please log in again.', detail: t };
                });
            }
            if (!response.ok) {
                return response.text().then(function (t) {
                    throw { status: response.status, message: 'API error ' + response.status, detail: t };
                });
            }
            return response.json();
        });
    }

    // ======================== UTIL ============================
    function escapeHtml(text) {
        if (!text) return '';
        var div = document.createElement('div');
        div.appendChild(document.createTextNode(text));
        return div.innerHTML;
    }

    function prettifyWs(key) {
        if (!key) return 'Workspace';
        return key.charAt(0).toUpperCase() + key.slice(1);
    }

    // Union of CONFIG.URLS + links already on the portal page
    function harvestUrls() {
        var seen = {};
        var out = [];

        function add(u) {
            if (!u) return;
            u = u.trim();
            if (!u || seen[u]) return;
            if (u.indexOf('kms.cloud.global') === -1) return;
            if (u.indexOf('/docs/') === -1) return;
            seen[u] = true;
            out.push(u);
        }

        CONFIG.URLS.forEach(add);

        try {
            var anchors = document.querySelectorAll(
                '#explore-products a[href], .product-box a[href], a[href*="/docs/"]'
            );
            for (var i = 0; i < anchors.length; i++) {
                add(anchors[i].getAttribute('href'));
            }
        } catch (e) {}

        return out;
    }

    // =================== PORTAL DASHBOARD =====================
    function initPortal() {
        whenReady(function () {
            if (currentMode !== 'portal') return; // navigated away during retry
            if (document.getElementById('doc360-portal-dashboard')) {
                portalTries = 0;
                return;
            }

            var anchor = document.getElementById('explore-products') ||
                document.querySelector('main') ||
                document.querySelector('.container-fluid');

            if (!anchor) {
                // Portal content renders async — retry before falling back
                if (portalTries < 30) {
                    portalTries++;
                    portalTimer = setTimeout(function () {
                        portalTimer = null;
                        initPortal();
                    }, 200);
                } else {
                    portalTries = 0;
                    mountPortal(document.body, true);
                }
                return;
            }

            portalTries = 0;
            mountPortal(anchor, false);
        });
    }

    function mountPortal(anchor, prepend) {
        if (document.getElementById('doc360-portal-dashboard')) return;

        var mount = document.createElement('div');
        mount.id = 'doc360-portal-dashboard';
        mount.className = 'doc360-portal-dashboard';

        if (anchor === document.body) {
            anchor.insertBefore(mount, prepend ? anchor.firstChild : null);
        } else {
            anchor.parentNode.insertBefore(mount, anchor);
        }

        if (!getToken()) {
            renderLoginBanner(mount);
            return;
        }

        mount.innerHTML =
            '<div class="doc360-loading">' +
                '<div class="doc360-spinner"></div>' +
                '<p>Loading your workspaces...</p>' +
            '</div>';

        var urls = harvestUrls();
        if (urls.length === 0) {
            mount.innerHTML = '<div class="doc360-empty">No documentation URLs configured.</div>';
            return;
        }

        var urlsKey = urls.join('|');
        if (portalCache && portalCache.urlsKey === urlsKey) {
            renderPortalDashboard(mount, portalCache.data);
            return;
        }

        fetchWorkspaces(urls).then(function (data) {
            portalCache = { urlsKey: urlsKey, data: data };
            if (document.getElementById('doc360-portal-dashboard') !== mount) return;
            renderPortalDashboard(mount, data);
        }).catch(function (err) {
            if (document.getElementById('doc360-portal-dashboard') !== mount) return;
            if (err && err.status === 401) {
                portalCache = null;
                mount.innerHTML = '';
                renderLoginBanner(mount, err.message);
            } else {
                mount.innerHTML =
                    '<div class="doc360-error">' +
                        '<p class="doc360-error-msg">' +
                            escapeHtml((err && err.message) || 'Failed to load content.') +
                        '</p>' +
                    '</div>';
            }
        });
    }

    function teardownPortal() {
        if (portalTimer) {
            clearTimeout(portalTimer);
            portalTimer = null;
        }
        portalTries = 0;
        var el = document.getElementById('doc360-portal-dashboard');
        if (el && el.parentNode) el.parentNode.removeChild(el);
    }

    function renderLoginBanner(mount, message) {
        var banner = document.createElement('div');
        banner.className = 'doc360-login-banner';
        banner.innerHTML =
            '<span class="doc360-login-banner-text">' +
                escapeHtml(message || 'Log in to access all workspaces you are authorized to view.') +
            '</span>' +
            '<a class="doc360-login-banner-btn" href="' + escapeHtml(CONFIG.LOGIN_URL) + '">Log in</a>';
        mount.appendChild(banner);
    }

    function renderPortalDashboard(mount, data) {
        mount.innerHTML = '';

        if (data && data.Message && !data.Success) {
            var msg = document.createElement('div');
            msg.className = 'doc360-empty';
            msg.textContent = data.Message;
            mount.appendChild(msg);
            return;
        }

        var results = (data && data.Workspaces) || [];
        var okResults = results.filter(function (r) { return r.Status === 'ok'; });

        if (okResults.length === 0) {
            var empty = document.createElement('div');
            empty.className = 'doc360-empty';
            empty.textContent = (data && data.Message) || 'No authorized workspace content available.';
            mount.appendChild(empty);
            return;
        }

        // Group by workspace
        var byWs = {};
        okResults.forEach(function (r) {
            var key = r.Workspace || 'other';
            if (!byWs[key]) byWs[key] = [];
            byWs[key].push(r);
        });
        var wsKeys = Object.keys(byWs);

        // Header
        var header = document.createElement('div');
        header.className = 'doc360-portal-header';
        header.innerHTML =
            '<h2 class="doc360-portal-title">All Workspaces</h2>' +
            '<p class="doc360-portal-sub">' +
                data.SuccessfulFetches + ' articles across ' + wsKeys.length +
                ' workspace' + (wsKeys.length === 1 ? '' : 's') +
            '</p>';
        mount.appendChild(header);

        // Tabs
        var tabs = document.createElement('div');
        tabs.className = 'doc360-tabs';
        var content = document.createElement('div');
        content.className = 'doc360-portal-content';

        wsKeys.forEach(function (key, idx) {
            var sample = byWs[key][0];
            var perm = (sample.Permission || '').toLowerCase();
            var tab = document.createElement('button');
            tab.type = 'button';
            tab.className = 'doc360-tab' + (idx === 0 ? ' active' : '');
            tab.innerHTML = escapeHtml(prettifyWs(key)) +
                ' <span class="doc360-count">' + byWs[key].length + '</span>' +
                ' <span class="doc360-perm doc360-perm-' + (perm === 'edit' ? 'edit' : 'read') + '">' +
                (perm === 'edit' ? 'Read & Edit' : 'Read-only') + '</span>';
            tab.addEventListener('click', function () {
                tabs.querySelectorAll('.doc360-tab').forEach(function (t) { t.classList.remove('active'); });
                tab.classList.add('active');
                renderArticleList(content, byWs[key]);
            });
            tabs.appendChild(tab);
        });

        mount.appendChild(tabs);
        mount.appendChild(content);
        renderArticleList(content, byWs[wsKeys[0]]);
    }

    // Article cards that NAVIGATE to the real Document360 article page
    function renderArticleList(container, items) {
        container.innerHTML = '';
        var list = document.createElement('div');
        list.className = 'doc360-article-grid';

        items.forEach(function (item) {
            var card = document.createElement('a');
            card.className = 'doc360-article-card';
            card.href = item.Url; // real navigation to Document360 article
            card.innerHTML =
                '<span class="doc360-article-title">' + escapeHtml(item.Title || 'Untitled') + '</span>' +
                '<span class="doc360-article-url">' + escapeHtml(item.Url) + '</span>' +
                '<span class="doc360-article-cta">Open article &rarr;</span>';
            list.appendChild(card);
        });

        container.appendChild(list);
    }

    // ====================== KB SIDEBAR ========================
    function renderPanelBody(body, data) {
        body.innerHTML = '';
        if (!data) return;

        if (data.Message && !data.Success) {
            var msg = document.createElement('div');
            msg.className = 'doc360-empty';
            msg.textContent = data.Message;
            body.appendChild(msg);
            return;
        }

        var results = data.Workspaces || [];
        if (results.length === 0) {
            var empty = document.createElement('div');
            empty.className = 'doc360-empty';
            empty.textContent = 'No content available.';
            body.appendChild(empty);
            return;
        }

        var header = document.createElement('div');
        header.className = 'doc360-header';
        header.innerHTML = '<span class="doc360-badge">' +
            data.SuccessfulFetches + '/' + data.TotalWorkspaces + ' articles loaded</span>';
        body.appendChild(header);

        var okResults = results.filter(function (r) { return r.Status === 'ok'; });
        var others = results.filter(function (r) { return r.Status !== 'ok'; });

        var byWs = {};
        okResults.forEach(function (r) {
            var key = r.Workspace || 'other';
            if (!byWs[key]) byWs[key] = [];
            byWs[key].push(r);
        });

        var wsKeys = Object.keys(byWs);
        if (wsKeys.length > 0) {
            var tabs = document.createElement('div');
            tabs.className = 'doc360-tabs';
            var content = document.createElement('div');
            content.className = 'doc360-tab-content';

            wsKeys.forEach(function (key, idx) {
                var sample = byWs[key][0];
                var perm = (sample.Permission || '').toLowerCase();
                var tab = document.createElement('button');
                tab.type = 'button';
                tab.className = 'doc360-tab' + (idx === 0 ? ' active' : '');
                tab.innerHTML = escapeHtml(prettifyWs(key)) +
                    ' <span class="doc360-perm doc360-perm-' + (perm === 'edit' ? 'edit' : 'read') + '">' +
                    (perm === 'edit' ? 'Read & Edit' : 'Read-only') + '</span>';
                tab.addEventListener('click', function () {
                    tabs.querySelectorAll('.doc360-tab').forEach(function (t) { t.classList.remove('active'); });
                    tab.classList.add('active');
                    renderTabContent(content, byWs[key]);
                });
                tabs.appendChild(tab);
            });

            body.appendChild(tabs);
            body.appendChild(content);
            renderTabContent(content, byWs[wsKeys[0]]);
        }

        if (others.length > 0) {
            var footer = document.createElement('div');
            footer.className = 'workspace-errors';
            var listHtml = '<h3>' + (others.some(function (o) { return o.Status === 'unauthorized'; })
                ? 'Access Not Granted' : 'Unavailable Content') + '</h3><ul>';
            others.forEach(function (o) {
                listHtml += '<li>' + escapeHtml(o.Url) +
                    (o.Error ? ' — ' + escapeHtml(o.Error) : '') + '</li>';
            });
            listHtml += '</ul>';
            footer.innerHTML = listHtml;
            body.appendChild(footer);
        }
    }

    function renderTabContent(container, items) {
        container.innerHTML = '';
        items.forEach(function (ws) {
            var section = document.createElement('div');
            section.className = 'doc360-workspace';
            section.innerHTML =
                '<div class="doc360-ws-header">' +
                    '<h2 class="doc360-ws-title">' + escapeHtml(ws.Title || 'Untitled') + '</h2>' +
                    '<div class="doc360-ws-url">' + escapeHtml(ws.Url) + '</div>' +
                '</div>' +
                '<div class="doc360-ws-body">' + (ws.HtmlContent || '') + '</div>';
            container.appendChild(section);
        });
    }

    function ensureStyles() {
        var href = CONFIG.API_BASE + '/Content/doc360-multiworkspace.css';
        if (document.getElementById('doc360-mw-css')) return;
        // Already added externally via Document360 Custom CSS?
        if (document.querySelector('link[href="' + href + '"]')) return;
        var link = document.createElement('link');
        link.id = 'doc360-mw-css';
        link.rel = 'stylesheet';
        link.href = href;
        document.head.appendChild(link);
    }

    function whenReady(fn) {
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', fn);
        } else {
            fn();
        }
    }

    function initKb() {
        whenReady(buildSidebar);
    }

    function buildSidebar() {
        if (currentMode !== 'kb') return;
        if (document.getElementById(CONFIG.PANEL_ID)) return;

        var btn = document.createElement('button');
        btn.id = CONFIG.BTN_ID;
        btn.type = 'button';
        btn.className = 'doc360-mw-trigger';
        btn.innerHTML = '&#9776; All Workspaces';
        document.body.appendChild(btn);

        var overlay = document.createElement('div');
        overlay.id = 'doc360-mw-overlay';
        overlay.className = 'doc360-mw-overlay';
        document.body.appendChild(overlay);

        var panel = document.createElement('aside');
        panel.id = CONFIG.PANEL_ID;
        panel.className = 'doc360-mw-panel';
        panel.innerHTML =
            '<div class="doc360-mw-panel-header">' +
                '<span class="doc360-mw-panel-title">All Workspaces</span>' +
                '<button type="button" class="doc360-mw-close" aria-label="Close">&times;</button>' +
            '</div>' +
            '<div class="doc360-mw-panel-body" id="doc360-mw-body"></div>';
        document.body.appendChild(panel);

        function open() {
            panel.classList.add('open');
            overlay.classList.add('open');
            loadContent();
        }
        function close() {
            panel.classList.remove('open');
            overlay.classList.remove('open');
        }

        btn.addEventListener('click', open);
        overlay.addEventListener('click', close);
        panel.querySelector('.doc360-mw-close').addEventListener('click', close);
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') close();
        });
    }

    function teardownKb() {
        [CONFIG.BTN_ID, 'doc360-mw-overlay', CONFIG.PANEL_ID].forEach(function (id) {
            var el = document.getElementById(id);
            if (el && el.parentNode) el.parentNode.removeChild(el);
        });
    }

    function loadContent() {
        var body = document.getElementById('doc360-mw-body');
        if (!body) return;
        if (loadedOnce && body.children.length > 0) return;

        body.innerHTML =
            '<div class="doc360-loading">' +
                '<div class="doc360-spinner"></div>' +
                '<p>Loading documentation...</p>' +
            '</div>';

        fetchWorkspaces(harvestUrls()).then(function (data) {
            loadedOnce = true;
            renderPanelBody(body, data);
        }).catch(function (err) {
            if (err && err.status === 401) {
                body.innerHTML =
                    '<div class="doc360-error">' +
                        '<p class="doc360-error-msg">' + escapeHtml(err.message) + '</p>' +
                        '<p><a href="' + escapeHtml(CONFIG.LOGIN_URL) + '">Log in again</a></p>' +
                    '</div>';
            } else {
                body.innerHTML =
                    '<div class="doc360-error">' +
                        '<p class="doc360-error-msg">' +
                            escapeHtml((err && err.message) || 'Failed to load content.') +
                        '</p>' +
                    '</div>';
            }
        });
    }

    // ==================== SPA ROUTING =========================
    // Document360 is an SPA: script runs once per full page load, but the
    // user can move portal ↔ article without reloading. Watch history
    // (works alongside script.js's own locationchange patch — route()
    // is idempotent and same-path events are ignored).
    function route() {
        captureToken();
        ensureStyles();

        var mode = detectMode();
        var pathChanged = location.pathname !== lastPath;
        lastPath = location.pathname;

        if (mode !== currentMode) {
            if (mode === 'portal') {
                teardownKb();
                currentMode = 'portal';
                initPortal();
            } else {
                teardownPortal();
                currentMode = 'kb';
                initKb();
            }
            return;
        }

        if (!pathChanged) return;
        if (mode === 'kb') {
            buildSidebar(); // no-op if already present
        } else if (!document.getElementById('doc360-portal-dashboard') && !portalTimer) {
            initPortal(); // SPA re-render wiped the mount — re-add
        }
    }

    function setupRouting() {
        var origPush = history.pushState;
        var origReplace = history.replaceState;
        history.pushState = function () {
            var r = origPush.apply(this, arguments);
            window.dispatchEvent(new Event('locationchange'));
            return r;
        };
        history.replaceState = function () {
            var r = origReplace.apply(this, arguments);
            window.dispatchEvent(new Event('locationchange'));
            return r;
        };
        window.addEventListener('popstate', function () {
            window.dispatchEvent(new Event('locationchange'));
        });

        var pending = null;
        window.addEventListener('locationchange', function () {
            // Debounce: script.js may also dispatch, and double-patching
            // would emit twice per navigation
            if (pending) clearTimeout(pending);
            pending = setTimeout(function () {
                pending = null;
                route();
            }, 50);
        });
    }

    // ========================= INIT ===========================
    function init() {
        captureToken();     // before patching history → silent URL cleanup
        ensureStyles();
        setupRouting();
        route();            // first mode init
    }

    init();
})();
