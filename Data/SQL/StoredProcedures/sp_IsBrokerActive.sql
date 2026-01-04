CREATE PROCEDURE sp_IsBrokerActive
    @BrokerId NVARCHAR(50),
    @IsActive BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1 
        FROM Brokers 
        WHERE BrokerId = @BrokerId 
            AND IsActive = 1 
            AND IsVerified = 1
    )
    BEGIN
        SET @IsActive = 1;
    END
    ELSE
    BEGIN
        SET @IsActive = 0;
    END
END
