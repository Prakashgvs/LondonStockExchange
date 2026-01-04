
CREATE NONCLUSTERED INDEX IX_Brokers_IsActive 
ON Brokers(IsActive) 
INCLUDE (BrokerId, BrokerName);