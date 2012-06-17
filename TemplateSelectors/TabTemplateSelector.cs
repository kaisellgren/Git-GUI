using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GG
{
    public class TabTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NormalTabTemplate { get; set; }
        public DataTemplate NewTabTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == CollectionView.NewItemPlaceholder)
            {
                return NewTabTemplate;
            }
            else
            {
                return NormalTabTemplate;
            }
        }
    }
}
