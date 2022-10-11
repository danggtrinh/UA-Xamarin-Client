using System;

namespace Thesis
{
    public class AppMasterDetailPageMenuItem
    {
        public AppMasterDetailPageMenuItem()
        {
            TargetType = typeof(AppMasterDetailPageDetail);
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public Type TargetType { get; set; }
    }
}