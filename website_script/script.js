(function () {

    console.log('\starting now')

    // Global reference for cached files
    let existingPdfFile = null;

    // Stop words to exclude from word cloud
    const stopWords = new Set([
        "the", "and", "for", "with", "from", "that", "this", "have", "has", "was", "were",
        "will", "shall", "would", "should", "can", "could", "may", "might", "about", "into",
        "onto", "your", "you", "are", "not", "but", "all", "any", "our", "out", "get", "had",
        "then", "than", "over", "after", "before", "when", "where", "which", "what", "why", "how",
        "system", "guide", "article", "overview", "configuration", "using", "user", "users", "page", "pages",
        "section", "resource", "resources", "click", "information", "documentation", "support", "software", "application",
        "applications", "product", "products", "service", "services", "feature", "features", "example", "examples", "default",
        "note", "notes", "step", "steps", "table"
    ]);

    // Utility to load an external script dynamically
    function loadScript(src) {
        return new Promise((resolve, reject) => {
            if (document.querySelector(`script[src="${src}"]`)) {
                resolve();
                return;
            }
            const script = document.createElement("script");
            script.src = src;
            script.async = true;
            script.onload = () => resolve();
            script.onerror = () => reject(new Error(`Failed to load script: ${src}`));
            document.head.appendChild(script);
        });
    }

    // --- PDF Button Functionality ---
    function getArticleIdFromRequests() {
        const entries = performance.getEntriesByType("resource");
        for (let entry of entries) {
            if (entry.name.includes("get-article-detail")) {
                const match = entry.name.match(/articleId=([a-f0-9-]+)/i);
                if (match) return match[1];
            }
        }
        return null;
    }



    function setupIfButtonExists() {
        const button = Array.from(
            document.querySelectorAll("button.dropdown-item")
        ).find((btn) => btn.textContent.trim() === "Export PDF");

        if (button && !button.dataset.pdfEnhanced) {

            const favoriteBtn = document.querySelector('.favorite-toggle-btn');
            const next = $('.favorite-toggle-btn').next();

            if (next.length) {
                next.hide();
            }
            setupExportButton(button);
            button.dataset.pdfEnhanced = "true";
            return true;
        }
        return false;
    }

    function initializePDFExport() {
        // Attempt instant setup
        setupIfButtonExists();
        injectInlinePdfButton();
    }

    function injectInlinePdfButton() {
        //nst followBtn = document.querySelector('.follow-button');
        //nst editarticlebtn = document.querySelector( '.article-more-options > .edit-article-btn');    

        const favoriteBtn = document.querySelector('.follow-button');
        if (!favoriteBtn || document.getElementById('exportPdfBtn')) return;

        const btn = document.createElement('button');
        btn.id = "exportPdfBtn";
        btn.innerText = "Export PDF";
        btn.className = "btn btn-primary";
        btn.style.marginRight = "10px";
        btn.onclick = function () {
            generateInlinePdf();
        };

        //followBtn.parentNode.insertBefore(btn, followBtn);

        //const favoriteBtn = document.querySelector('.favorite-toggle-btn');

        if (favoriteBtn) {
            //favoriteBtn.parentNode.insertBefore(btn, favoriteBtn);
            //favoriteBtn.insertAdjacentElement('afterend', btn);
            // Create wrapper exactly like Document360's structure
            debugger;
            const wrapper = document.createElement('span');

            wrapper.appendChild(btn);

            // Insert wrapper after the Follow wrapper
            const followWrapper = favoriteBtn.parentElement;
            console.log(followWrapper);
            followWrapper.insertAdjacentElement('afterend', wrapper);
        }
        console.log("✅ Inline Export PDF button added");
        /*
        if(editarticlebtn){       
           console.log('edit here')
           editarticlebtn.insertAdjacentElement('afterend', btn);	
        }
        else{   
            console.log('followup')
           followBtn.parentElement.after(btn);
        }*/
        console.log("✅ Inline Export PDF button added");

    }

    function injectInlinePdfButton1() {
        const favoriteBtn = document.querySelector('.me-2');

        if (!favoriteBtn) return;

        // Prevent binding multiple times
        if (favoriteBtn.dataset.pdfOverride === "true") return;

        favoriteBtn.dataset.pdfOverride = "true";

        favoriteBtn.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            generateInlinePdf();
        }, true);

        console.log("✅ Existing .me-2 button click overridden");

    }

    function generateInlinePdf() {
        const url = window.location.href;

        // Show notification
        const notification = document.createElement("div");
        notification.innerText = "Your file is downloading...";
        notification.style.cssText = `
    position: fixed;
    top: 20px;
    right: 20px;
    background: #222;
    color: #fff;
    padding: 12px 20px;
    border-radius: 6px;
    font-size: 14px;
    z-index: 99999;
    box-shadow: 0 4px 12px rgba(0,0,0,0.2);
  `;

        document.body.appendChild(notification);

        fetch("https://1up.co.in/NEXUS_API/api/BusinessPdf/GenerateFromUrl", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                Url: url,
                ForceRefresh: true
            })
        })
            .then(res => {
                if (!res.ok) throw new Error("API failed");
                return res.blob();
            })
            .then(blob => {
                const h1Element = document.querySelector('h1.article-title');
                const articleName = h1Element ? h1Element.textContent.trim() : "Article";
                const safeName = articleName.replace(/[\\/:*?"<>|]/g, "").replace(/\s+/g, "_");
                const link = document.createElement("a");
                link.href = URL.createObjectURL(blob);
                link.download = safeName + ".pdf";
                link.click();

                // Update notification
                notification.innerText = "Your file has been downloaded.";

                setTimeout(() => {
                    notification.remove();
                    URL.revokeObjectURL(link.href);
                }, 3000);
            })
            .catch(err => {
                console.error("❌ PDF generation failed", err);

                notification.innerText = "Failed to generate PDF.";

                setTimeout(() => {
                    notification.remove();
                }, 3000);
            });
    }

    async function setupExportButton(button) {
        button.addEventListener("click", async (event) => {
            try {
                event.preventDefault();
                event.stopImmediatePropagation();

                button.disabled = true;
                button.textContent = "Preparing PDF...";

                const h1Element = document.querySelector('h1.article-title');
                const text = h1Element ? h1Element.textContent.trim() : "Article";

                const pathname = window.location.pathname;
                const filename = pathname.split("/").pop();
                existingPdfFile = await searchFileByName(filename);

                if (existingPdfFile) {

                    downloadFile(existingPdfFile.file_url, filename);
                    return;
                }


                const article = document.querySelector('[id*="article"]');
                if (!article) throw new Error("Article content not found");

                const response = await fetch("https://1up.co.in/NEXUS_API/api/BusinessPdf/GeneratePdf", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ htmlContent: article.outerHTML })
                });

                if (!response.ok) throw new Error("Failed to generate PDF");

                const blob = await response.blob();
                const link = document.createElement("a");
                link.href = URL.createObjectURL(blob);
                link.download = text + ".pdf";
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
            } catch (error) {
                console.error("PDF export failed:", error);
            } finally {
                button.disabled = false;
                button.textContent = "Export PDF";
            }
        }, true);
    }

    async function searchFileByName(filename) {
        try {
            const url = `https://apihub.document360.io/v2/Drive/Search?searchKeyword=${encodeURIComponent(filename)}&pageNo=0&take=20&allowImagesOnly=false`;
            const response = await fetch(url, {
                method: "GET",
                headers: {
                    "api_token": "qstNVgrrO9A6w9byiy2c/n4Cza4lkaOLmXX9KXSx6yH4/0FDSBdOeSnP40bamD7jaJegf5sb0azs9GH1aOALH1qxM74IHURyIdvhC2ijW9tmHyc5TuLG5KOYibNfRaQ0mzMIffNJzcFqff5TtUFb8w==",
                    "accept": "application/json"
                }
            });
            const result = await response.json();
            if (response.ok && result.success && result.data?.all_files?.length > 0) {
                return result.data.all_files[0];
            }
            return null;
        } catch (err) {
            console.error("❌ Error searching for file:", err);
            return null;
        }
    }

    function downloadFile(fileUrl, fileName) {
        window.open(fileUrl, "_blank");
    }

    // --- WordCloud Functionality ---
    function generateWordCloud() {

        const article = document.querySelector('#articleContent');
        if (!article) {

            return false;
        }

        const container = document.querySelector(".resources-section.expanded") || document.querySelector(".resources-section");
        if (!container) return false;


        const text = article.innerText.toLowerCase();
        const words = text.match(/\b[a-z]{4,}\b/g);
        if (!words) return false;

        const frequency = {};
        words.forEach(word => {
            if (!stopWords.has(word)) {
                frequency[word] = (frequency[word] || 0) + 1;
            }
        });
        console.log('Step4');
        const list = Object.entries(frequency).sort((a, b) => b[1] - a[1]).slice(0, 60);
        if (list.length === 0) return false;

        const oldCanvas = document.querySelector("#wordcloud-canvas");
        if (oldCanvas) oldCanvas.remove();

        const canvas = document.createElement("canvas");
        canvas.id = "wordcloud-canvas";
        canvas.width = container.clientWidth || 800;
        canvas.height = 350;
        canvas.style.marginBottom = "10px";
        container.insertAdjacentElement("beforebegin", canvas);

        if (typeof WordCloud === "function") {
            WordCloud(canvas, {
                list,
                gridSize: 8,
                weightFactor(size) { return Math.min(size * 5, 45); },
                rotateRatio: 1.8,
                minRotation: 0,
                maxRotation: Math.PI / 4,
                shuffle: true,
                drawOutOfBound: false,
                shrinkToFit: true,
                fontFamily: "Poppins",
                color() {
                    const colors = ["#1C4A71", "#2F6B99", "#90C058", "#444444"];
                    return colors[Math.floor(Math.random() * colors.length)];
                },
                backgroundColor: "#fff"
            });
            console.log("✅ WordCloud generated successfully");
            return true;
        }
        return false;
    }

    let cloudTimeout;
    function safeGenerateWordCloud(retries = 8) {
        clearTimeout(cloudTimeout);
        cloudTimeout = setTimeout(() => {
            const success = generateWordCloud();
            if (!success && retries > 0) {
                console.log(`Resource mapping delayed. Retrying wordcloud... (${retries} left)`);
                safeGenerateWordCloud(retries - 1);
            }
        }, 400);
    }

    async function initWordCloud() {
        try {
            await loadScript("https://cdnjs.cloudflare.com/ajax/libs/wordcloud2.js/1.0.1/wordcloud2.min.js");
            safeGenerateWordCloud();
        } catch (err) {
            console.error("❌ Failed to load WordCloud2.js", err);
        }
    }

    function setupPortalSearch() {
        // old markup (legacy input+button)
        const oldInput = document.getElementById("portalSearch");
        const oldBtn = document.getElementById("portalSearchBtn");
        if (oldBtn && oldInput && !oldBtn.dataset.searchBound) {
            oldBtn.dataset.searchBound = "true";
            oldBtn.addEventListener("click", function () {
                let query = oldInput.value.trim();
                if (query) window.location.href = `https://kms.cloud.global/nomadix/en/search?q=${encodeURIComponent(query)}`;
            });
            oldInput.addEventListener("keypress", function (e) {
                if (e.key === "Enter") oldBtn.click();
            });
        }

        // new markup (CTRL+K trigger button → opens modal)
        const newBtn = document.querySelector('button.search-box');
        if (newBtn && !newBtn.dataset.searchBound) {
            newBtn.dataset.searchBound = "true";
            newBtn.addEventListener("click", function () {
                setTimeout(bindSearchModalInput, 200); // wait for modal to render
            });
        }
    }

    function bindSearchModalInput() {
        const modalInput = document.querySelector('input[type="search"], input[placeholder*="Search" i]');
        if (!modalInput || modalInput.dataset.searchBound) return;
        modalInput.dataset.searchBound = "true";
        modalInput.addEventListener("keypress", function (e) {
            if (e.key === "Enter") {
                const query = modalInput.value.trim();
                if (query) window.location.href = `https://kms.cloud.global/nomadix/en/search?q=${encodeURIComponent(query)}`;
            }
        });
    }

    // --- Search Portal Engine ---
    function setupPortalSearch1() {
        try {
            const btn = document.getElementById("portalSearchBtn");
            console.log(btn)
            const input = document.getElementById("portalSearch");
            console.log(input)
            if (!btn || !input || btn.dataset.searchBound) return;

            console.log('here1');
            btn.dataset.searchBound = "true";

            console.log('here2');
            btn.addEventListener("click", function () {
                let query = input.value.trim();
                if (query) window.location.href = `https://kms.cloud.global/nomadix/en/search?q=${encodeURIComponent(query)}`;
            });
            console.log('here3');
            input.addEventListener("keypress", function (e) {
                if (e.key === "Enter") btn.click();
            });
            console.log('here4');
        } catch (err) {
            console.error("❌ Failed to load WordCloud2.js", err);
        }
    }

    // --- Blockquotes Design Modifiers ---
    function updateBlockquoteDesigns() {
        document.querySelectorAll("blockquote[data-background='#1C4A71']").forEach(blockquote => {
            if (blockquote.classList.contains("custom-info-box")) return;
            blockquote.classList.add("custom-info-box");
            buildBlockquoteMarkup(blockquote, "custom-info-content");
        });

        document.querySelectorAll("blockquote[data-background='#fdf2ce']").forEach(blockquote => {
            if (blockquote.classList.contains("custom-warning-box")) return;
            blockquote.classList.add("custom-warning-box");
            buildBlockquoteMarkup(blockquote, "custom-warning-content");
        });

        document.querySelectorAll("p emoji[data-name='information_source']").forEach(emoji => emoji.replaceWith(""));
    }

    function buildBlockquoteMarkup(blockquote, contentClass) {
        const wrapper = document.createElement("div");
        wrapper.classList.add(contentClass);

        const infoIcon = document.createElement("div");
        infoIcon.classList.add("info-icon");

        const img = document.createElement("img");
        img.src = "https://files.document360.io/456bc41e-6bf6-4ebf-a659-426e82b994f7/Images/Documentation/notes.png";
        img.alt = "Info";
        img.width = 48;
        img.height = 48;
        infoIcon.appendChild(img);

        const infoText = document.createElement("div");
        infoText.classList.add("info-text");
        infoText.innerHTML = blockquote.innerHTML;
        infoText.querySelectorAll("img").forEach(img => img.remove());

        blockquote.innerHTML = "";
        wrapper.appendChild(infoIcon);
        wrapper.appendChild(infoText);
        blockquote.appendChild(wrapper);
    }

    function updateLogo() {
        const logoImg = document.querySelector('.brand-logo img');
        if (!logoImg) return;
        const pathname = window.location.pathname;

        if (pathname.includes('/trustedwifi')) {
            logoImg.src = 'https://files.document360.io/456bc41e-6bf6-4ebf-a659-426e82b994f7/Images/Documentation/TrustedWifi-Logo_blue.png';
        } else if (pathname === '/' || pathname.startsWith('/docs') || pathname.startsWith('/nomadix')) {
            logoImg.src = 'https://files.document360.io/456bc41e-6bf6-4ebf-a659-426e82b994f7/Images/Documentation/Nomadix%20Nexus%20portal%20logo.png';
        }

        const brandLogos = document.getElementsByClassName("brand-logo");
        Array.from(brandLogos).forEach(logo => logo.removeAttribute("href"));
    }

    // --- Dom Mutations Aggregator ---
    // A clean persistent observer that handles dynamic UI updates across layout routing switches safely.
    let lastActiveHref = "";

    function filterCategoryTree() {

        // =====================================================
        // ONLY /default/
        // =====================================================

        if (!window.location.pathname.startsWith('/default/')) {
            return;
        }

        const $container = $('.cdk-virtual-scroll-content-wrapper').first();

        if (!$container.length) {
            return;
        }

        const $nodes = $container.children('[id^="node-"]');

        if (!$nodes.length) {
            return;
        }


        // =====================================================
        // GET CURRENT URL
        // =====================================================

        const currentPath = window.location.pathname
            .split('?')[0]
            .split('#')[0]
            .replace(/\/$/, '');


        // =====================================================
        // READ TREE
        // =====================================================

        const items = [];

        $nodes.each(function () {

            const $node = $(this);
            const $wrapper = $node.children('.tree-wrapper');
            const $link = $wrapper.find('a.data-title').first();

            if (!$wrapper.length || !$link.length) {
                return;
            }

            const href = ($link.attr('href') || '')
                .split('?')[0]
                .split('#')[0]
                .replace(/\/$/, '');

            const level = $wrapper.children('.filler').length;

            items.push({
                node: $node,
                wrapper: $wrapper,
                link: $link,
                level: level,
                href: href,
                active: $wrapper.hasClass('active')
            });

        });


        // =====================================================
        // FIND ACTIVE BY URL
        // =====================================================

        let activeIndex = items.findIndex(function (item) {
            return item.href === currentPath;
        });


        // Fallback to .active
        if (activeIndex === -1) {

            activeIndex = items.findIndex(function (item) {
                return item.active;
            });

        }


        // IMPORTANT:
        // If Angular is currently rebuilding the tree,
        // DO NOT HIDE ANYTHING.
        if (activeIndex === -1) {
            console.log('FILTER: waiting for active node...');
            return;
        }


        const active = items[activeIndex];
        const activeLevel = active.level;


        console.log(
            'FILTER ACTIVE:',
            active.link.text().trim(),
            'LEVEL:',
            activeLevel
        );


        // =====================================================
        // FIND THE ANCESTOR AT EACH LEVEL
        // =====================================================

        const ancestors = {};

        ancestors[activeLevel] = activeIndex;


        for (let level = activeLevel - 1; level >= 0; level--) {

            for (let i = activeIndex - 1; i >= 0; i--) {

                if (items[i].level === level) {

                    ancestors[level] = i;
                    break;
                }
            }
        }


        console.log(
            'ANCESTORS:',
            Object.keys(ancestors).map(function (level) {

                return {
                    level: level,
                    text: items[ancestors[level]]
                        .link
                        .text()
                        .trim()
                };

            })
        );


        // =====================================================
        // FIRST HIDE EVERYTHING
        // =====================================================

        items.forEach(function (item) {
            item.node.hide();
        });


        // =====================================================
        // HELPER:
        // SHOW ALL ITEMS OF A LEVEL UNDER A PARTICULAR PARENT
        // =====================================================

        function showLevelUnderParent(level, parentLevel, parentIndex) {

            for (let i = parentIndex + 1; i < items.length; i++) {

                // We reached outside this parent's branch
                if (items[i].level <= parentLevel) {
                    break;
                }

                // Direct children of parent
                if (items[i].level === level) {
                    items[i].node.show();
                }
            }
        }


        // =====================================================
        // SHOW ROOT ACTIVE PARENT
        // =====================================================

        const rootIndex = ancestors[0];

        if (rootIndex !== undefined) {
            items[rootIndex].node.show();
        }


        // =====================================================
        // SHOW LEVEL-1 SIBLINGS
        //
        // HYATT
        // ├── Phase 1
        // └── Phase 2
        // =====================================================

        if (activeLevel >= 1) {

            const parent0 = ancestors[0];

            if (parent0 !== undefined) {

                showLevelUnderParent(
                    1,
                    0,
                    parent0
                );

            }
        }


        // =====================================================
        // SHOW LEVEL-2 SIBLINGS
        //
        // Phase 2
        // ├── Nomadix
        // ├── Gateway ACTIVE
        // ├── Ruckus
        // └── Hardware
        // =====================================================

        if (activeLevel >= 2) {

            const parent1 = ancestors[1];

            if (parent1 !== undefined) {

                showLevelUnderParent(
                    2,
                    1,
                    parent1
                );

            }
        }


        // =====================================================
        // SHOW LEVEL-3 SIBLINGS
        //
        // If the active item is a level-3 child,
        // show its level-3 siblings.
        // =====================================================

        if (activeLevel >= 3) {

            const parent2 = ancestors[2];

            if (parent2 !== undefined) {

                showLevelUnderParent(
                    3,
                    2,
                    parent2
                );

            }
        }


        // =====================================================
        // SHOW ACTIVE ITEM'S CHILDREN / GRANDCHILDREN
        //
        // THIS IS THE IMPORTANT PART
        // =====================================================

        for (let i = activeIndex + 1; i < items.length; i++) {

            // Once we reach the active item's level or above,
            // its subtree has ended.
            if (items[i].level <= activeLevel) {
                break;
            }

            items[i].node.show();
        }


        // =====================================================
        // DEBUG
        // =====================================================

        console.log('========== FINAL TREE ==========');

        items.forEach(function (item, index) {

            console.log(
                index,
                'LEVEL:',
                item.level,
                'ACTIVE:',
                item.active,
                'VISIBLE:',
                item.node.is(':visible'),
                'TEXT:',
                item.link.text().trim()
            );

        });

        console.log('========== END TREE ==========');
    }
    const globalDomObserver = new MutationObserver(() => {

        filterCategoryTree();

        initializePDFExport();
        setupPortalSearch();
        updateLogo();

        // ...

        const active = $('.tree-wrapper.active');

        if (active.length) {

            console.log("Observer:", active.text().trim());

            const href = active.attr('href');

            if (href !== lastActiveHref) {

                lastActiveHref = href;

                console.log("Tree updated:", href);

                setTimeout(() => {
                    filterCategoryTree();
                    // collapseTree();
                }, 50);
            }
        }
    });
    // --------------------------------------------------
    // PRESERVE TREE SCROLL POSITION ON CATEGORY CLICK
    // --------------------------------------------------

    $(document)
        .off('click.treeScroll', '.tree-wrapper a')
        .on('click.treeScroll', '.tree-wrapper a', function () {

            const viewport = document.querySelector(
                '.cdk-virtual-scroll-viewport'
            );

            if (!viewport) return;

            const scrollTop = viewport.scrollTop;

            console.log('Saving tree scroll:', scrollTop);

            // Restore after Document360 changes the route/tree
            setTimeout(() => {
                viewport.scrollTop = scrollTop;
            }, 50);

            setTimeout(() => {
                viewport.scrollTop = scrollTop;
            }, 150);

            setTimeout(() => {
                viewport.scrollTop = scrollTop;
            }, 300);
        });
    $(document)
        .off('click.treeFilter', '.tree-arrow')
        .on('click.treeFilter', '.tree-arrow', function () {

            console.log('Chevron clicked');

            setTimeout(() => {
                filterCategoryTree();
            }, 100);
        });
    // --- Initializers & Core SPAs Event Hooks ---
    document.addEventListener("DOMContentLoaded", () => {
        globalDomObserver.observe(document.body, { childList: true, subtree: true });

        // Redirect logic
        if (document.referrer === 'https://trustedwifi.staging.cloud.global/') {
            window.location = "https://kms.cloud.global/trustedwifi/en";
            return;
        }
        document.body.style.visibility = "visible";

        initializePDFExport();
        setupPortalSearch();
    });

    window.addEventListener("load", () => {
        initWordCloud();

    });

    // Native History API interceptors to hook SPA routing shifts cleanly
    (function (history) {
        const push = history.pushState;
        history.pushState = function () {
            push.apply(history, arguments);
            window.dispatchEvent(new Event('locationchange'));
        };
        const replace = history.replaceState;
        history.replaceState = function () {
            replace.apply(history, arguments);
            window.dispatchEvent(new Event('locationchange'));
        };
    })(window.history);

    window.addEventListener('popstate', () => window.dispatchEvent(new Event('locationchange')));

    function collapseTree() {

        const active = $('.tree-wrapper.active');

        if (!active.length)
            return;

        let currentLevel = active.children('.filler').length;

        const keepOpen = [];

        active.parent().prevAll().each(function () {

            const wrapper = $(this).children('.tree-wrapper');

            if (!wrapper.length)
                return;

            if (wrapper.hasClass('default-category')) {

                const level = wrapper.children('.filler').length;

                if (level < currentLevel) {

                    keepOpen.push(wrapper[0]);
                    currentLevel = level;

                }
            }
        });

        console.log(
            "Keep:",
            keepOpen.map(x =>
                $(x).find('a').first().text().trim()
            )
        );

        $('.tree-arrow[aria-expanded="true"]').each(function () {
            console.log(
                "Expanded:",
                $(this).closest('.tree-wrapper').find('a').first().text().trim()
            );

            const wrapper = $(this).closest('.tree-wrapper');

            if (!keepOpen.includes(wrapper[0])) {
                this.click();
            }

        });

    }

    function waitForTree() {

        let tries = 0;

        const timer = setInterval(() => {

            tries++;

            const active = $('.tree-wrapper.active');

            if (active.length) {

                clearInterval(timer);

                console.log("✅ Tree Ready");

                collapseTree();

            }

            if (tries > 30) {
                clearInterval(timer);
                console.log("❌ Tree not ready");
            }

        }, 100);
    }



    window.addEventListener('locationchange', () => {

        console.log("📍 SPA Route Change Detected");
        // Fire wordcloud verification with multiple runtime cycles to catch async elements
        safeGenerateWordCloud();
        // Allow macro-task queue prioritization for framework renders
        setTimeout(() => {
            initializePDFExport();
            setupPortalSearch();
            updateBlockquoteDesigns();
            //waitForTree();      
        }, 100);
    });


})();


// Keep standalone globally bound functions safely outside logic isolation scope if called directly via inline elements
function openModalWithIframe(url) {
    const existingModal = document.getElementById("dynamicFeedbackModal");
    if (existingModal) existingModal.remove();

    const modal = document.createElement("div");
    modal.id = "dynamicFeedbackModal";
    Object.assign(modal.style, {
        position: "fixed", top: 0, left: 0, width: "100%", height: "100%",
        background: "rgba(0,0,0,0.5)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 9999,
    });

    const content = document.createElement("div");
    Object.assign(content.style, {
        background: "#fff", borderRadius: "8px", width: "90%", maxWidth: "900px", height: "80%", display: "flex", flexDirection: "column",
    });

    const closeBtn = document.createElement("button");
    closeBtn.textContent = "Close";
    Object.assign(closeBtn.style, { alignSelf: "flex-end", margin: "10px", padding: "5px 15px", cursor: "pointer" });
    closeBtn.addEventListener("click", () => modal.remove());

    const formsUrl = "https://forms.microsoft.com/Pages/ResponsePage.aspx?id=Im7vUN0VQUC7SNHHE06lPafW6nSXDiVPr1DHnNIBeSpUNE85U1FPQ01MRUNJWFdITlpIM0dGOTg4TC4u&embed=true";
    const iframe = document.createElement("iframe");
    iframe.src = formsUrl;
    Object.assign(iframe.style, { border: "none", flex: 1, width: "100%", maxWidth: "790px", margin: "0 auto" });

    content.appendChild(closeBtn);
    content.appendChild(iframe);
    modal.appendChild(content);
    document.body.appendChild(modal);

    modal.addEventListener("click", (event) => {
        if (event.target === modal) modal.remove();
    });
}

document.addEventListener("click", function (e) {
    const contactLink = e.target.closest('.nav-bar-nav .action-item[href="https://kms.cloud.global/shared/4192f9f8-1475-4a88-81e4-570faad4390c"]');
    if (!contactLink) return;
    e.preventDefault();
    openModalWithIframe();
});

function fnOpen() {
    const modal = document.getElementById("feedbackModal");
    const closeBtn = document.getElementById("closeModal");
    if (!modal || !closeBtn) return;
    modal.style.display = "flex";
    closeBtn.addEventListener("click", () => modal.style.display = "none", { once: true });
}