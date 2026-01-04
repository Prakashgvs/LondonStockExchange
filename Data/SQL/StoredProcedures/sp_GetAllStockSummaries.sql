CREATE PROCEDURE sp_GetAllStockSummaries
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TickerSymbol,
        TotalTradeValue AS TotalValue,
        TotalShares,
        TransactionCount,
        LastUpdatedUtc AS LastUpdated
    FROM StockSummary;
END;
