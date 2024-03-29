﻿namespace Hashgraph.Portal.Services;

public class DefaultsService
{
    public Address? Payer { get; set; }
    public Gateway? Gateway { get; set; }
    public long FeeLimit { get; set; } = 200_000_000;
    public TimeSpan TransactionDuration { get; set; } = TimeSpan.FromSeconds(120);
    public TimeSpan ReceiptWaitDuration { get; set; } = TimeSpan.FromSeconds(60);
    public int ReceiptRetryCount { get; set; } = 30;
}