using System;
using System.Collections.Generic;
using System.Text;

namespace ModernExpo.SelfCheckout.Entities.Models
{
    public class Tag
    {
        public int Id { get; set; }

        public string Key { get; set; }

        public string RuleType { get; set; }

        public string RuleValue { get; set; }

        public bool IsRuleValidated { get; set; }

        public Tag()
        {
            IsRuleValidated = true;
        }
    }
}
