﻿using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Model.Wrappers;
using System;

namespace RoxorBot.Data.Model
{
    public class FilterWrapper : WrapperBase<Filter>
    {
        public Guid Id => Model.Id;
        public string Word { get { return GetValue<string>(); } set { SetValue(value); } }
        //TODO: change to timespan
        public int BanDuration { get { return GetValue<int>(); } set { SetValue(value); } }
        public string Author { get { return GetValue<string>(); } set { SetValue(value); } }
        public bool IsRegex { get { return GetValue<bool>(); } set { SetValue(value); } }
        public bool IsWhitelist { get { return GetValue<bool>(); } set { SetValue(value); } }

        public FilterWrapper(Filter model) : base(model)
        {
        }
    }
}
