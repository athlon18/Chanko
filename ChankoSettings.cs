using System;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using ff14bot.Managers;
using ff14bot.Helpers;


namespace ChankoPlugin
{
    public partial class ChankoSettings : Form
    {
        private Dictionary<uint, string> foodDict;

        public ChankoSettings()
        {
            foodDict = new Dictionary<uint, string>();
            InitializeComponent();

            UpdateFood();

            foodDropBox.SelectedValue = Settings.Instance.Id;
        }

        private void foodDropBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Instance.FoodName = foodDict[(uint)foodDropBox.SelectedValue];
            Settings.Instance.Id = (uint)foodDropBox.SelectedValue;
            Settings.Instance.Save();
        }

        private void foodDropBox_Click(object sender, EventArgs e)
        {
            UpdateFood();
        }

        private void UpdateFood()
        {
            foodDict.Clear();

            foreach (var item in InventoryManager.FilledSlots.GetFoodItems())
            {
                foodDict[item.TrueItemId] = "(" + item.Count + ")" + item.EnglishName + (item.IsHighQuality ? " HQ" : "");
            }

            foodDropBox.DataSource = new BindingSource(foodDict, null);
            foodDropBox.DisplayMember = "Value";
            foodDropBox.ValueMember = "Key";
        }
    }
}
