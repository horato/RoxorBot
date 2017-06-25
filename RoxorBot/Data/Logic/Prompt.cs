﻿using System.Windows.Forms;

namespace RoxorBot.Data.Logic
{
    public static class Prompt
    {
        public static string ShowDialog(string text, string caption)
        {
            Form prompt = new Form();
            prompt.Width = 500;
            prompt.Height = 150;
            prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
            prompt.Text = caption;
            prompt.StartPosition = FormStartPosition.CenterScreen;
            Label textLabel = new Label { Left = 50, Top = 20, Text = text, Width = 200 };
            TextBox textBox = new TextBox { Left = 50, Top = 50, Width = 400 };
            Button confirmation = new Button { Text = "Ok", Left = 350, Width = 100, Top = 70 };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            prompt.ShowDialog();
            return textBox.Text;
        }
        public static bool Ask(string text, string caption)
        {
            DialogResult result = MessageBox.Show(text,caption,MessageBoxButtons.YesNo);
            return (result == DialogResult.Yes);
        }
    }
}
