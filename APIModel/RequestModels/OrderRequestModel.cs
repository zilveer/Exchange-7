using Entity;
using Entity.Partials;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Util;

namespace APIModel.RequestModels
{
    public class OrderRequestModel : IValidatableObject
    {
        private readonly ITimeProvider _timeProvider;
        public OrderRequestModel()
        {
            _timeProvider = new TimeProvider();
        }

        public OrderRequestModel(ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        [Required]
        public short? MarketId { get; set; }

        [Required]
        public decimal? Rate { get; set; }

        public decimal? StopRate { get; set; }

        [Required]
        public decimal? Quantity { get; set; }

        [Required]
        public bool? IsBuy { get; set; }

        [Required]
        public OrderType? OrderType { get; set; }

        [Required]
        public OrderCondition? OrderCondition { get; set; }

        [Required]
        public bool? IsIcebergOrger { get; set; }

        public decimal? IcebergQuantity { get; set; }

        public DateTime? CancelOn { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();

            if (MarketId <= 0)
                errors.Add(new ValidationResult("Invalid MarketId.", new[] { nameof(MarketId) }));

            if ((int)OrderType <= (int)Entity.OrderType.Limit && (int)OrderType >= (int)Entity.OrderType.StopLoss)
                errors.Add(new ValidationResult("Invalid OrderType.", new[] { nameof(OrderType) }));

            if (Rate <= 0 && OrderType == Entity.OrderType.Limit)
                errors.Add(new ValidationResult("Rate should be greater than 0.", new[] { nameof(Rate) }));

            if (Rate != 0 && OrderType == Entity.OrderType.Market)
                errors.Add(new ValidationResult("Rate should be 0.", new[] { nameof(Rate) }));

            if (StopRate <= 0 && OrderType == Entity.OrderType.StopLimit)
                errors.Add(new ValidationResult("Invalid Stop Rate.", new[] { nameof(StopRate) }));

            if (IsIcebergOrger == true && IcebergQuantity == null)
                errors.Add(new ValidationResult("Please enter Iceberg Quantity.", new[] { nameof(IcebergQuantity) }));

            if (IsIcebergOrger == true && IcebergQuantity <= Quantity)
                errors.Add(new ValidationResult("Invalid Iceberg Quantity.", new[] { nameof(IcebergQuantity) }));

            if (CancelOn < _timeProvider.GetUtcDateTime().AddMinutes(5))
                errors.Add(new ValidationResult("Good till date should be future time.", new[] { nameof(CancelOn) }));

            if (OrderCondition == Entity.Partials.OrderCondition.BookOrCancel && OrderType == Entity.OrderType.Market)
                errors.Add(new ValidationResult("Market order can not be book or cancel.", new[] { nameof(OrderCondition), nameof(OrderType) }));

            return errors;
        }
    }
}
