﻿using PO;
using System;
using System.Windows.Controls;

namespace PL.ViewModels
{
    public enum PanelType
    {
        Display,
        List,
        Add,
        Other,
    }

    //I changed here to make it work from ContentControl to UserControl
    public record Panel(PanelType PanelType, ContentControl View, string Header, bool CanClose = true);

    /// <summary>
    /// Defines All Workspace conf
    /// </summary>
    static class Workspace
    {
        public static string EntityPanelName(string name, int? id = null) => id == null ? $"Add {name}" : $"{name} #{id}";

        public static string DronePanelName(int? id = null) => EntityPanelName(nameof(PO.Drone), id);

        public static string ParcelPanelName(int? id = null) => EntityPanelName(nameof(PO.Parcel), id);

        public static string CustomerPanelName(int? id = null) => EntityPanelName(nameof(PO.Customer), id);

        public static string BaseStationPanelName(int? id = null) => EntityPanelName(nameof(PO.BaseStation), id);

        public static string CustomerSentListName(int id) => $"Customer {id} Sent Parcels";

        public static string CustomerRecievedListName(int id) => $"Customer {id} Recieved Parcels";

        public static string StationChrgedDronesName(int id) => $"Station #{id} Charged Drones";

        public static string ListPanelName(Type type) => $"{type.Name}s";

        public static Panel DronePanel(int? id = null) => id == null 
            ? new(PanelType.Add, new Views.AddDroneView(), DronePanelName()) 
            : new(PanelType.Display, new Views.DroneDetailsView((int)id), DronePanelName(id));

        public static Panel ParcelPanel(int? id = null) => id == null 
            ? new(PanelType.Add, new Views.AddParcelView(), ParcelPanelName())
            : new(PanelType.Display, new Views.ParcelDetailsView((int)id), ParcelPanelName(id));

        public static Panel CustomerPanel(int? id = null, bool canClose = true) => id == null 
            ? new(PanelType.Add, new Views.AddCustomerView(), CustomerPanelName(), canClose)
            : new(PanelType.Display, new Views.CustomerDetailsView((int)id), CustomerPanelName(id), canClose);

        public static Panel BaseStationPanel(int? id = null) => id == null
            ? new(PanelType.Add, new Views.AddStationView(), BaseStationPanelName())
            : new(PanelType.Display, new Views.StationDetailsView((int)id), BaseStationPanelName(id));

        public static Panel FilteredDronesListPanel(Predicate<PO.DroneForList> predicate, string header, bool canClose = true) =>
            new(PanelType.List, new Views.FilteredDronesListView(predicate), header, canClose);

        public static Panel FilteredParcelsListPanel(Predicate<PO.ParcelForList> predicate, string header, bool canClose = true) =>
            new(PanelType.List, new Views.FilteredParcelsListView(predicate), header, canClose);

        public static Panel ParcelsListPanel => new(PanelType.List, new Views.ParcelsListView(), ListPanelName(typeof(Parcel)), false);
        
        public static Panel DronesListPanel => new(PanelType.List, new Views.DronesListView(), ListPanelName(typeof(Drone)), false);
        
        public static Panel CustomersListPanel => new(PanelType.List, new Views.CustomersListView(), ListPanelName(typeof(Customer)), false);
        
        public static Panel BaseStationsListPanel => new(PanelType.List, new Views.BaseStationsListView(), ListPanelName(typeof(BaseStation)), false);

        public static Panel MainMapPanel => new(PanelType.Other, new Views.MainMapView(), "Main Map", false);


        public static RelayCommand<Panel> AddPanelCommand => Views.WorkspaceWindow.AddPanelCommand;

        public static RelayCommand<string> RemovePanelCommand => Views.WorkspaceWindow.RemovePanelCommand;

        public static readonly string TargerNameOfListPanelType = "ListArea";
    }
}
