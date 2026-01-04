CREATE PROCEDURE sp_GetStockSummariesByTickers
(
    @Tickers TickerSymbolTableType READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        s.TickerSymbol,
        s.TotalTradeValue AS TotalValue,
        s.TotalShares,
        s.TransactionCount,
        s.LastUpdatedUtc AS LastUpdated
    FROM StockSummary s
    INNER JOIN @Tickers t
        ON s.TickerSymbol = t.TickerSymbol;
END;
