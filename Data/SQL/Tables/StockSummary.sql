CREATE TABLE StockSummary
(
    TickerSymbol NVARCHAR(10) PRIMARY KEY,
    TotalShares DECIMAL(18,4) NOT NULL,
    TotalTradeValue DECIMAL(18,4) NOT NULL,
    LastUpdatedUtc DATETIME2 NOT NULL
);
