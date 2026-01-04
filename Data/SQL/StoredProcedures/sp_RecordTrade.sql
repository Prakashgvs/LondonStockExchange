CREATE PROCEDURE sp_RecordTrade
(
    @TradeId UNIQUEIDENTIFIER,
    @TickerSymbol NVARCHAR(10),
    @Price DECIMAL(18,4),
    @Shares DECIMAL(18,4),
    @BrokerId NVARCHAR(50),
    @TimestampUtc DATETIME2,
    @TradeValue DECIMAL(18,4),
    @TransactionId BIGINT OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Idempotency check (safe under concurrency)
        IF EXISTS (SELECT 1 FROM Trades WHERE TradeId = @TradeId)
        BEGIN
            SELECT @TransactionId = TransactionId
            FROM Trades
            WHERE TradeId = @TradeId;

            COMMIT TRANSACTION;
            RETURN;
        END

        -- Insert trade
        INSERT INTO Trades
        (
            TradeId,
            TickerSymbol,
            Price,
            Shares,
            BrokerId,
            TradeValue,
            TimestampUtc
        )
        VALUES
        (
            @TradeId,
            @TickerSymbol,
            @Price,
            @Shares,
            @BrokerId,
            @TradeValue,
            @TimestampUtc
        );

        SET @TransactionId = SCOPE_IDENTITY();

        -- Update aggregate
        MERGE StockSummary AS target
        USING (SELECT @TickerSymbol AS TickerSymbol) AS source
        ON target.TickerSymbol = source.TickerSymbol
        WHEN MATCHED THEN
            UPDATE SET
                TotalShares = target.TotalShares + @Shares,
                TotalTradeValue = target.TotalTradeValue + @TradeValue,
                LastUpdatedUtc = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN
            INSERT (TickerSymbol, TotalShares, TotalTradeValue, LastUpdatedUtc)
            VALUES (@TickerSymbol, @Shares, @TradeValue, SYSUTCDATETIME());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
