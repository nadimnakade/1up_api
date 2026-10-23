CREATE OR ALTER PROCEDURE [dbo].[USP_GET_EMPLOYEE_EXTENSION_API]
    @EmpCode INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        d.EMP_CODE        AS EmpCode,
        d.DID_NUMBER      AS DidNumber,
        d.EXTENSION_NUMBER AS Extension,
        d.DID_STATUS      AS Status,
        d.BASE_URL        AS BaseUrl,
        d.CLICK2CALL_PATH AS Click2CallPath,
        d.TOKEN_ID        AS TokenId,
        d.EXTENSION_PASSWORD AS ExtensionPassword,
        d.SIP_SERVER      AS SipServer,
        d.SIP_PORT        AS SipPort,
        d.SIP_PASSWORD    AS SipPassword,
        d.SIP_REALM       AS SipRealm
    FROM TBL_CIP_EMPLOYEE_DID d
    WHERE d.IS_DELETED = 0
      AND d.DID_STATUS = 'Active'
      AND (
          d.EMP_CODE = @EmpCode
          OR d.EMP_CODE IS NULL
      )
    ORDER BY 
        CASE WHEN d.EMP_CODE = @EmpCode THEN 0 ELSE 1 END
END
