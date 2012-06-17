using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GG
{
    class RepoTabContentTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// This method determines what template to use for tab's content.
        /// 
        /// For example, if repo is not opened, it shows New Tab template, otherwise it shows the repository template.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            string templateName;

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                templateName = "repoTabContentTemplate";
            }
            else if (item == CollectionView.NewItemPlaceholder)
            {
                templateName = "repoTabDashboardContentTemplate";
            }
            else
            {
                RepositoryViewModel repository = item as RepositoryViewModel;

                templateName = repository.NotOpened ? "repoTabDashboardContentTemplate" : "repoTabContentTemplate";
            }

            FrameworkElement element = container as FrameworkElement;
            var template = element.TryFindResource(templateName) as DataTemplate;
            if (template != null)
            {
                return template;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
