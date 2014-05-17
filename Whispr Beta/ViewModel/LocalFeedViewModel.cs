using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using WhisprBeta.Common;
using WhisprBeta.Design;
using WhisprBeta.Services;

namespace WhisprBeta.ViewModel
{
    public class LocalFeedViewModel : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public int RadiusMeters { get; set; }
        public ObservableCollection<Message> Messages { get; private set; }
        private IMessageService messageService;
        private ILocationService locationService;
        private IStatusService statusService;

        /// <summary>
        /// Parameterless constructor is for designers (Blend, Visual Studio designer)
        /// </summary>
        public LocalFeedViewModel() : this(new DesignMessageService(), new DesignLocationService(), new StatusService())
        {
            Title = "Hello Binding!";
            RadiusMeters = 100;
            if (DesignerProperties.IsInDesignTool)
            {
                RefreshMessages();
            }
        }

        public LocalFeedViewModel(IMessageService messageService, ILocationService locationService, IStatusService statusService)
        {
            this.messageService = messageService;
            this.locationService = locationService;
            this.statusService = statusService;
            Messages = new ObservableCollection<Message>();
        }

        private async Task RefreshMessages()
        {
            Messages.Clear();
            var messages = await messageService.Get(locationService.UserLocation, RadiusMeters);
            foreach (var message in messages)
            {
                Messages.Add(message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
