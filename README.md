# London Stock API (.NET)

A REST API for the **London Stock Exchange** that receives trade notifications from authorised brokers and exposes updated stock prices.

---

## Features

- **Record Trades** — Receive real-time trade notifications with ticker symbol, price, number of shares, and broker ID.  
- **Stock Valuations** — Query current stock values calculated as weighted average prices.  
- **Optimised Performance** — Dual-table design with pre-computed aggregates for constant-time lookups.

---

## Architecture

The API uses a **dual-table design pattern** for performance and ACID consistency.

### Tables Overview

#### Trades Table
- Acts as an *append-only ledger* storing all trades.

#### StockSummary Table
- Contains *pre-computed aggregates* for fast average price calculations:  
  - `SUM(price × shares)`  
  - `SUM(shares)`

### Why This Design?

| Approach | Read Performance | Write Overhead | Consistency |
|-----------|------------------|----------------|--------------|
| Dual-table (chosen) | O(1) | Low | ACID guaranteed |
| Query-time aggregation | O(n) | None | Always consistent |
| Materialised views | O(1) | Background refresh | Eventual |

### Weighted Average Calculation

\[
\text{averagePrice} = \frac{\text{totalValue}}{\text{totalShares}}
\]

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)  
- SQL Server  
- MSMQ enabled on Windows  

---

## Quick Start

### Build

```
dotnet build
```

### Run

```
dotnet run --project LondonStock.Api
```

API runs at: [http://localhost:5000](http://localhost:5000)

---

## API Endpoints

### Record a Trade

**POST** `/api/trades`  
**Content-Type:** `application/json`

**Request Body:**
```
{
"tickerSymbol": "VOD",
"price": 1.245,
"shares": 1250.75,
"brokerId": "BRK-LDN-007"
}
```


**Response (201 Created):**

```
{
"transactionId": 42,
"tickerSymbol": "VOD",
"price": 1.245,
"shares": 1250.75,
"brokerId": "BRK-LDN-007",
"tradeValue": 1557.18,
"timestamp": "2024-01-15T10:31:12"
}
```

---

### Get Stock Value

**GET** `/api/stocks/{ticker}/value`  

**Example:**  
`GET /api/stocks/VOD/value`

**Response (200 OK):**

```
{
"tickerSymbol": "VOD",
"averagePrice": 1.2384,
"totalShares": 98500.75,
"transactionCount": 312,
"lastUpdated": "2024-01-15T10:31:12"
}
```

---

### Get All Stock Values

**GET** `/api/stocks/values`

**Response (200 OK):**

```
[
{
"tickerSymbol": "VOD",
"averagePrice": 1.2384,
"totalShares": 98500.75,
"transactionCount": 312,
"lastUpdated": "2024-01-15T10:31:12"
},
{
"tickerSymbol": "BP",
"averagePrice": 4.9121,
"totalShares": 210400.00,
"transactionCount": 184,
"lastUpdated": "2024-01-15T10:29:45"
}
]
```

---

### Get Specific Stock Values

**GET** `/api/stocks/values?tickers=VOD,BP,HSBA`

---

## Data Model

### Trade

| Field | Type | Description |
|--------|------|-------------|
| id | long | Auto-generated primary key |
| tickerSymbol | string (10) | Stock ticker symbol |
| price | decimal | Price per share in GBP |
| shares | decimal | Number of shares (can be decimal) |
| brokerId | string (50) | Authorised broker identifier |
| timestamp | DateTime | Trade timestamp |

### StockSummary

| Field | Type | Description |
|--------|------|-------------|
| tickerSymbol | string (10) | Primary key |
| totalValue | decimal | SUM(price × shares) |
| totalShares | decimal | SUM(shares) |
| transactionCount | long | Number of trades |
| lastUpdated | DateTime | Last update timestamp |

---

## Error Handling

All errors return a consistent JSON structure:

```
{
"timestamp": "2024-01-15T10:32:10",
"status": 400,
"error": "Validation Failed",
"message": "Invalid request data",
"details": {
"price": "Price must be greater than 0",
"tickerSymbol": "Ticker symbol is required"
}
}
```

---

## Configuration

Edit `appsettings.json`:

```
{
"ConnectionStrings": {
"MsSqlDatabase": "Server=localhost;Database=LondonStock;Trusted_Connection=True;"
},
"Msmq": {
"TradeQueue": ".\private$\stock-trades"
}
}
```

---

## Enhancement Ideas (Scaling)

- **Caching** — Introduce Redis to cache frequently accessed stock summaries (/stocks/{ticker}/value) with short TTLs.
Use cache-aside strategy and invalidate cache entries on successful trade processing to ensure consistency.  
- **Message Processing** — Horizontally scale MSMQ consumers to handle higher trade volumes. or migrate to Azure Service Bus.  
- **Database** — Add read replicas or partition summary tables.  
- **Rate Limiting** — Apply per-broker request limits.  
- **Authentication** — Use JWT-based broker authentication.  
- **Monitoring** — Integrate Azure Application Insights and Azure Monitor dashboards.

---

## Assumptions & Design Decisions

- Trades are immutable once recorded (append-only ledger).
- Stock prices are calculated using all historical trades (no time-windowing).
- Broker authorization is assumed to be handled via upstream authentication (JWT/API Gateway).
- Decimal precision is used for price and shares to avoid floating-point inaccuracies.
- MSMQ guarantees at-least-once delivery; idempotency is handled at the database layer.
