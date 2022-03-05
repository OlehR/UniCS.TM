using System;
using System.Collections.Generic;
using System.Text;

namespace ModernIntegration.Model
{
    public enum CustomWindowInputType
    {
        Buttons = 0,
        Input = 1,
        Html = 2
    }

public class CustomWindowInputSettings
    {
        public string Placeholder { get; set; }

        public int MinInputLength { get; set; }

        public string ActionNamePrefix { get; set; }

        public string ActionNameSuffix { get; set; }
    }


public class CustomWindowButton
    {
        public CustomWindowButton() { }
        public CustomWindowButton(string displayName, string actionData)
        {
            DisplayName = displayName;
            ActionData = actionData;
        }
        public string DisplayName { get; set; }

        public string ActionData { get; set; }
    }



public class CustomWindowModel
    {
        public CustomWindowModel()
        {
            Caption = string.Empty;
            Text = string.Empty;
            Buttons = new List<CustomWindowButton>();
            AnswerRequired = false;
        }

        public string Caption { get; set; }

        public string Text { get; set; }

        public bool AnswerRequired { get; set; }

        public CustomWindowInputType Type { get; set; }

        public CustomWindowInputSettings InputSettings { get; set; }

        public List<CustomWindowButton> Buttons { get; set; }
    }



public class TerminalCustomWindowModel
    {
        public Guid TerminalId { get; set; }
        public CustomWindowModel CustomWindow { get; set; }

        public TerminalCustomWindowModel()
        {
            TerminalId = Guid.Empty;
            CustomWindow = new CustomWindowModel();
        }

        public TerminalCustomWindowModel(Guid terminalId, CustomWindowModel customWindow)
        {
            TerminalId = terminalId;
            CustomWindow = customWindow;
        }
    }

}
