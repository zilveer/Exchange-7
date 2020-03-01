using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Entity
{
    public partial class ExchangeContext : DbContext
    {
        public virtual DbSet<Balance> Balance { get; set; }
        public virtual DbSet<Currency> Currency { get; set; }
        public virtual DbSet<DepositWithdrawRequest> DepositWithdrawRequest { get; set; }
        public virtual DbSet<KycInfo> KycInfo { get; set; }
        public virtual DbSet<Log> Log { get; set; }
        public virtual DbSet<Market> Market { get; set; }
        public virtual DbSet<Orders> Orders { get; set; }
        public virtual DbSet<OrdersTransaction> OrdersTransaction { get; set; }
        public virtual DbSet<ReceivingAddress> ReceivingAddress { get; set; }
        public virtual DbSet<SendReceiveTransaction> SendReceiveTransaction { get; set; }
        public virtual DbSet<SendingAddress> SendingAddress { get; set; }
        public virtual DbSet<Trade> Trade { get; set; }
        public virtual DbSet<TradeFeeHistory> TradeFeeHistory { get; set; }
        public virtual DbSet<TradeHistory> TradeHistory { get; set; }
        public virtual DbSet<TradeTransaction> TradeTransaction { get; set; }
        public virtual DbSet<Transaction> Transaction { get; set; }
        public virtual DbSet<UserCredentials> UserCredentials { get; set; }
        public virtual DbSet<Users> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Balance>(entity =>
            {
                entity.ToTable("balance");

                entity.HasIndex(e => new { e.UserId, e.CurrencyId })
                    .HasName("ux_user_id_currency_id")
                    .IsUnique()
                    .HasFilter("(is_deleted = false)");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.CurrencyId).HasColumnName("currency_id");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.LockedBalance).HasColumnName("locked_balance");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.SpendableBalance).HasColumnName("spendable_balance");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Balance)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("balance_currency_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Balance)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("balance_user_id_fkey");
            });

            modelBuilder.Entity<Currency>(entity =>
            {
                entity.ToTable("currency");

                entity.HasIndex(e => e.Code)
                    .HasName("ux_currency_code")
                    .IsUnique()
                    .HasFilter("(is_deleted = false)");

                entity.HasIndex(e => e.Name)
                    .HasName("ux_name")
                    .IsUnique()
                    .HasFilter("(is_deleted = false)");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasColumnName("code")
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.DailyMaxDepositAmount).HasColumnName("daily_max_deposit_amount");

                entity.Property(e => e.DailyMaxWithdrawAmount).HasColumnName("daily_max_withdraw_amount");

                entity.Property(e => e.DepositAllowed).HasColumnName("deposit_allowed");

                entity.Property(e => e.DepositHaltedReason)
                    .HasColumnName("deposit_halted_reason")
                    .HasMaxLength(2048);

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.IsVirtual).HasColumnName("is_virtual");

                entity.Property(e => e.MaxDepositAmount).HasColumnName("max_deposit_amount");

                entity.Property(e => e.MaxWithdrawAmount).HasColumnName("max_withdraw_amount");

                entity.Property(e => e.MinDepositAmount).HasColumnName("min_deposit_amount");

                entity.Property(e => e.MinWithdrawAmount).HasColumnName("min_withdraw_amount");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(250);

                entity.Property(e => e.WithdrawHaltedReason)
                    .HasColumnName("withdraw_halted_reason")
                    .HasMaxLength(2048);

                entity.Property(e => e.WithdrawalAllowed).HasColumnName("withdrawal_allowed");
            });

            modelBuilder.Entity<DepositWithdrawRequest>(entity =>
            {
                entity.ToTable("deposit_withdraw_request");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.CurrencyId).HasColumnName("currency_id");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.RequestStatus).HasColumnName("request_status");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.DepositWithdrawRequest)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("deposit_withdraw_request_currency_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.DepositWithdrawRequest)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("deposit_withdraw_request_user_id_fkey");
            });

            modelBuilder.Entity<KycInfo>(entity =>
            {
                entity.ToTable("kyc_info");

                entity.HasIndex(e => e.UserId)
                    .HasName("ux_user_id")
                    .IsUnique()
                    .HasFilter("(is_deleted = false)");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('kyc_id_seq'::regclass)");

                entity.Property(e => e.AdhaarNumber)
                    .HasColumnName("adhaar_number")
                    .HasMaxLength(50);

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.FirstName)
                    .HasColumnName("first_name")
                    .HasMaxLength(250);

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.LastName)
                    .HasColumnName("last_name")
                    .HasMaxLength(250);

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.PanNumber)
                    .HasColumnName("pan_number")
                    .HasMaxLength(50);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.KycInfo)
                    .HasForeignKey<KycInfo>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("kyc_info_user_id_fkey");
            });

            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("log");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.LogLevel).HasColumnName("log_level");

                entity.Property(e => e.LongMessage)
                    .HasColumnName("long_message")
                    .HasColumnType("character varying");

                entity.Property(e => e.ShortMessage)
                    .HasColumnName("short_message")
                    .HasColumnType("character varying");
            });

            modelBuilder.Entity<Market>(entity =>
            {
                entity.ToTable("market");

                entity.HasIndex(e => new { e.TradeCurrencyId, e.QuoteCurrencyId })
                    .HasName("ux_trade_currency_quote_currency")
                    .IsUnique()
                    .HasFilter("(is_deleted = false)");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.BookOrCancelAllowed).HasColumnName("book_or_cancel_allowed");

                entity.Property(e => e.CancelAllowed).HasColumnName("cancel_allowed");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.FillOrKillOrderAllowed).HasColumnName("fill_or_kill_order_allowed");

                entity.Property(e => e.GoodTillDateOrderAllowed).HasColumnName("good_till_date_order_allowed");

                entity.Property(e => e.ImmediateOrCancelAllowed).HasColumnName("immediate_or_cancel_allowed");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.MarketOrderAllowed).HasColumnName("market_order_allowed");

                entity.Property(e => e.MaxRate)
                    .HasColumnName("max_rate")
                    .HasColumnType("numeric(18,8)");

                entity.Property(e => e.MinRate)
                    .HasColumnName("min_rate")
                    .HasColumnType("numeric(18,8)");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.NewOrderAllowed).HasColumnName("new_order_allowed");

                entity.Property(e => e.QuantityStepSize).HasColumnName("quantity_step_size");

                entity.Property(e => e.QuoteCurrencyId).HasColumnName("quote_currency_id");

                entity.Property(e => e.RateStepSize)
                    .HasColumnName("rate_step_size")
                    .HasColumnType("numeric(18,8)");

                entity.Property(e => e.StopLimitOrderAllowed).HasColumnName("stop_limit_order_allowed");

                entity.Property(e => e.StopLossOrderAllowed).HasColumnName("stop_loss_order_allowed");

                entity.Property(e => e.TradeCurrencyId).HasColumnName("trade_currency_id");

                entity.Property(e => e.TradeFeeId).HasColumnName("trade_fee_id");

                entity.Property(e => e.TradeMaxQuantity).HasColumnName("trade_max_quantity");

                entity.Property(e => e.TradeMinQuantity).HasColumnName("trade_min_quantity");

                entity.Property(e => e.TradingEnd)
                    .HasColumnName("trading_end")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.TradingHaltedReason)
                    .HasColumnName("trading_halted_reason")
                    .HasMaxLength(2048);

                entity.Property(e => e.TradingStart)
                    .HasColumnName("trading_start")
                    .HasColumnType("timestamp with time zone");

                entity.HasOne(d => d.QuoteCurrency)
                    .WithMany(p => p.MarketQuoteCurrency)
                    .HasForeignKey(d => d.QuoteCurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("market_quote_currency_fkey");

                entity.HasOne(d => d.TradeCurrency)
                    .WithMany(p => p.MarketTradeCurrency)
                    .HasForeignKey(d => d.TradeCurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("market_trade_currency_fkey");

                entity.HasOne(d => d.TradeFee)
                    .WithMany(p => p.Market)
                    .HasForeignKey(d => d.TradeFeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("market_trade_fee_id_fkey");
            });

            modelBuilder.Entity<Orders>(entity =>
            {
                entity.ToTable("orders");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('order_id_seq'::regclass)");

                entity.Property(e => e.CancelOn)
                    .HasColumnName("cancel_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.CancelledOn)
                    .HasColumnName("cancelled_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Cost).HasColumnName("cost");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Fee).HasColumnName("fee");

                entity.Property(e => e.FeeCurrencyId).HasColumnName("fee_currency_id");

                entity.Property(e => e.IcebergQuantity).HasColumnName("iceberg_quantity");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.LockedBalance).HasColumnName("locked_balance");

                entity.Property(e => e.MarketId).HasColumnName("market_id");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.OrderCondition).HasColumnName("order_condition");

                entity.Property(e => e.OrderStatus).HasColumnName("order_status");

                entity.Property(e => e.OrderType).HasColumnName("order_type");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.QuantityExecuted).HasColumnName("quantity_executed");

                entity.Property(e => e.QuantityRemaining).HasColumnName("quantity_remaining");

                entity.Property(e => e.Rate)
                    .HasColumnName("rate")
                    .HasColumnType("numeric(18,8)");

                entity.Property(e => e.Side).HasColumnName("side");

                entity.Property(e => e.StopRate)
                    .HasColumnName("stop_rate")
                    .HasColumnType("numeric(18,8)");

                entity.Property(e => e.TradeFeeId).HasColumnName("trade_fee_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.TradeFee)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.TradeFeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("orders_trade_fee_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("order_user_id_fkey");
            });

            modelBuilder.Entity<OrdersTransaction>(entity =>
            {
                entity.ToTable("orders_transaction");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrdersTransaction)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("orders_transaction_order_id_fkey");

                entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.OrdersTransaction)
                    .HasForeignKey(d => d.TransactionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("orders_transaction_transaction_id_fkey");
            });

            modelBuilder.Entity<ReceivingAddress>(entity =>
            {
                entity.ToTable("receiving_address");

                entity.HasIndex(e => new { e.Address, e.CurrencyId })
                    .HasName("ux_address_id_currency_id")
                    .IsUnique()
                    .HasFilter("(is_deleted = false)");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasColumnName("address")
                    .HasMaxLength(250);

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.CurrencyId).HasColumnName("currency_id");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.PartitionId).HasColumnName("partition_id");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.ReceivingAddress)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("receiving_address_currency_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ReceivingAddress)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("receiving_address_user_id_fkey");
            });

            modelBuilder.Entity<SendReceiveTransaction>(entity =>
            {
                entity.ToTable("send_receive_transaction");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasColumnName("address")
                    .HasMaxLength(250);

                entity.Property(e => e.BlockchainTransactionId)
                    .HasColumnName("blockchain_transaction_id")
                    .HasMaxLength(1024);

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("time with time zone");

                entity.Property(e => e.CurrencyId).HasColumnName("currency_id");

                entity.Property(e => e.Fee).HasColumnName("fee");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.ReceivingAddressId).HasColumnName("receiving_address_id");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.SendReceiveTransaction)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("send_receive_transaction_currency_id_fkey");

                entity.HasOne(d => d.ReceivingAddress)
                    .WithMany(p => p.SendReceiveTransaction)
                    .HasForeignKey(d => d.ReceivingAddressId)
                    .HasConstraintName("send_receive_transaction_receiving_address_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SendReceiveTransaction)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("send_receive_transaction_user_id_fkey");
            });

            modelBuilder.Entity<SendingAddress>(entity =>
            {
                entity.ToTable("sending_address");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasColumnName("address")
                    .HasMaxLength(250);

                entity.Property(e => e.CreatedOn).HasColumnName("created_on");

                entity.Property(e => e.CurrencyId).HasColumnName("currency_id");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ModifiedOn).HasColumnName("modified_on");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasColumnName("name")
                    .HasMaxLength(250);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.SendingAddress)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("sending_address_currency_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.SendingAddress)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("sending_address_user_id_fkey");
            });

            modelBuilder.Entity<Trade>(entity =>
            {
                entity.ToTable("trade");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Fee).HasColumnName("fee");

                entity.Property(e => e.FeeCurrencyId).HasColumnName("fee_currency_id");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.IsMaker).HasColumnName("is_maker");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.OrderId).HasColumnName("order_id");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.Rate)
                    .HasColumnName("rate")
                    .HasColumnType("numeric(18,8)");

                entity.HasOne(d => d.FeeCurrency)
                    .WithMany(p => p.Trade)
                    .HasForeignKey(d => d.FeeCurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("trade_fee_currency_id_fkey");

                entity.HasOne(d => d.Order)
                    .WithMany(p => p.Trade)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("trade_order_id_fkey");
            });

            modelBuilder.Entity<TradeFeeHistory>(entity =>
            {
                entity.ToTable("trade_fee_history");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasColumnName("label")
                    .HasMaxLength(250);

                entity.Property(e => e.MakerFeePercent)
                    .HasColumnName("maker_fee_percent")
                    .HasColumnType("numeric(6,4)");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(2048);

                entity.Property(e => e.TakerFeePercent)
                    .HasColumnName("taker_fee_percent")
                    .HasColumnType("numeric(6,4)");
            });

            modelBuilder.Entity<TradeHistory>(entity =>
            {
                entity.ToTable("trade_history");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AskOrderId).HasColumnName("ask_order_id");

                entity.Property(e => e.BidOrderId).HasColumnName("bid_order_id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.MakerOrderId).HasColumnName("maker_order_id");

                entity.Property(e => e.MarketId).HasColumnName("market_id");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Quantity).HasColumnName("quantity");

                entity.Property(e => e.Rate)
                    .HasColumnName("rate")
                    .HasColumnType("numeric(18,8)");

                entity.Property(e => e.TakerOrderId).HasColumnName("taker_order_id");
            });

            modelBuilder.Entity<TradeTransaction>(entity =>
            {
                entity.ToTable("trade_transaction");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.TradeId).HasColumnName("trade_id");

                entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

                entity.HasOne(d => d.Trade)
                    .WithMany(p => p.TradeTransaction)
                    .HasForeignKey(d => d.TradeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("trade_transaction_trade_id_fkey");

                entity.HasOne(d => d.Transaction)
                    .WithMany(p => p.TradeTransaction)
                    .HasForeignKey(d => d.TransactionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("trade_transaction_transaction_id_fkey");
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transaction");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AbsoluteAmount).HasColumnName("absolute_amount");

                entity.Property(e => e.Amount).HasColumnName("amount");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.CurrencyId).HasColumnName("currency_id");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Note)
                    .HasColumnName("note")
                    .HasMaxLength(250);

                entity.Property(e => e.TransactionType).HasColumnName("transaction_type");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Currency)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.CurrencyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("transaction_currency_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Transaction)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("transaction_user_id_fkey");
            });

            modelBuilder.Entity<UserCredentials>(entity =>
            {
                entity.ToTable("user_credentials");

                entity.HasIndex(e => e.TwoFactorKey)
                    .HasName("user_credentials.ux_two_factor_key")
                    .IsUnique()
                    .HasFilter("((is_deleted = false) AND (two_factor_enabled = true))");

                entity.HasIndex(e => e.UserId)
                    .HasName("user_credentials.ux_user_id")
                    .IsUnique()
                    .HasFilter("(is_deleted = false)");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.LoginFailedAttempt).HasColumnName("login_failed_attempt");

                entity.Property(e => e.LoginFailedStartOn)
                    .HasColumnName("login_failed_start_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.LoginUnblockOn)
                    .HasColumnName("login_unblock_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasColumnName("password_hash")
                    .HasMaxLength(250);

                entity.Property(e => e.Salt)
                    .IsRequired()
                    .HasColumnName("salt")
                    .HasMaxLength(250);

                entity.Property(e => e.TwoFactorEnabled).HasColumnName("two_factor_enabled");

                entity.Property(e => e.TwoFactorKey)
                    .HasColumnName("two_factor_key")
                    .HasMaxLength(1024);

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.UserCredentials)
                    .HasForeignKey<UserCredentials>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_credentials_user_id_fkey");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users");

                entity.HasIndex(e => e.UniqueId)
                    .HasName("users_unique_id_key")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('user_id_seq'::regclass)");

                entity.Property(e => e.CreatedOn)
                    .HasColumnName("created_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.DebitBlockedReason)
                    .HasColumnName("debit_blocked_reason")
                    .HasMaxLength(2048);

                entity.Property(e => e.Email)
                    .HasColumnName("email")
                    .HasMaxLength(250);

                entity.Property(e => e.FreezedReason)
                    .HasColumnName("freezed_reason")
                    .HasMaxLength(2048);

                entity.Property(e => e.IsClosed).HasColumnName("is_closed");

                entity.Property(e => e.IsDebitAllowed).HasColumnName("is_debit_allowed");

                entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");

                entity.Property(e => e.IsFreezed).HasColumnName("is_freezed");

                entity.Property(e => e.MobileNumber)
                    .HasColumnName("mobile_number")
                    .HasMaxLength(20);

                entity.Property(e => e.ModifiedOn)
                    .HasColumnName("modified_on")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.UniqueId).HasColumnName("unique_id");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
