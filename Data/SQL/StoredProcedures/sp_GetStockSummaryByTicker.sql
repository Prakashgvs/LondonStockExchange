CREATE PROCEDURE sp_GetStockSummaryByTicker
(
    @TickerSymbol NVARCHAR(10)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        TickerSymbol,
        TotalTradeValue AS TotalValue,
        TotalShares,
        TransactionCount,
        LastUpdatedUtc AS LastUpdated
    FROM StockSummary
    WHERE TickerSymbol = @TickerSymbol;
END;
