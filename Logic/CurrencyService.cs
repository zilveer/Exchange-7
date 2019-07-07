﻿using System;
using System.Collections.Generic;
using System.Text;
using DataAccess.UnitOfWork;
using Entity;

namespace Logic
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReadOnlyContext _readonlyContext;
        public CurrencyService(IUnitOfWork unitOfWork, IReadOnlyContext readonlyContext)
        {
            _unitOfWork = unitOfWork;
            _readonlyContext = readonlyContext;
        }

        public List<Currency> GetCurrencies()
        {
            return _readonlyContext.CurrencyRepository.GetAllCurrencies();
        }
    }
}