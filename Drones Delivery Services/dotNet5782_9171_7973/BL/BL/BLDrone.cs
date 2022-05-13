﻿using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BL
{
    partial class BL 
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddDrone(int id, string model, WeightCategory maxWeight, int stationId)
        {
            var drone = new Drone()
            {
                Id = id,
                Model = model,
                MaxWeight = maxWeight,
                Battery = 0,
                Location = GetBaseStation(stationId).Location,
                ParcelInDeliver = null,
                State = DroneState.Maintenance,
            };

            lock(Dal) try
            {
                Dal.Add(new DO.Drone()
                {
                    Id = drone.Id,
                    Model = drone.Model,
                    MaxWeight = (DO.WeightCategory)drone.MaxWeight,
                });
            }
            catch (DO.IdAlreadyExistsException)
            {
                throw new IdAlreadyExistsException(typeof(Drone), id);
            }

            drones.Add(
                new DroneForList()
                {
                    Id = drone.Id,
                    Model = drone.Model,
                    MaxWeight = drone.MaxWeight,
                    Battery = drone.Battery,
                    Location = drone.Location.Clone(),
                    DeliveredParcelId = null,
                    State = drone.State,
                }
            );

            Dal.AddDroneCharge(drone.Id, stationId);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Drone GetDrone(int id)
        {
            DroneForList drone = drones.FirstOrDefault(d => d.Id == id);

            if (drone == null)
                throw new ObjectNotFoundException(typeof(Drone));

            ParcelInDeliver parcelInDeliver = drone.State == DroneState.Deliver 
                                              ? GetParcelInDeliver((int)drone.DeliveredParcelId) 
                                              : null;
            return new Drone()
            {
                Id = drone.Id,
                Battery = drone.Battery,
                Location = drone.Location.Clone(),
                MaxWeight = drone.MaxWeight,
                Model = drone.Model,
                State = drone.State,
                ParcelInDeliver = parcelInDeliver,
            };
        }

        public DroneForList GetDroneForList(int id)
        {
            return GetDroneForListRef(id).Clone();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IEnumerable<DroneForList> GetDronesList()
        {
            return drones.Select(drone => drone.Clone());
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetDroneBaseStation(int droneId)
        {
            try
            {
                return Dal.GetSingle<DO.DroneCharge>(c => c.DroneId == droneId).StationId;
            }
            catch (DO.ObjectNotFoundException)
            {
                throw new ObjectNotFoundException("Drone is not being charged");
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RenameDrone(int droneId, string model)
        {
            DroneForList drone = GetDroneForListRef(droneId);
            drone.Model = model;

            lock(Dal) try
            {
                Dal.Update<DO.Drone>(droneId, nameof(drone.Model), model);
            }
            catch (DO.ObjectNotFoundException e)
            {
                throw new ObjectNotFoundException(typeof(Drone), e);
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int ChargeDrone(int droneId)
        {
            DroneForList drone = GetDroneForListRef(droneId);

            if (drone.State != DroneState.Free)
            {
                throw new InvalidActionException("Can not send a non-free drone to charge");
            }

            var availableBaseStations = GetAvailableBaseStationsId();
            if (!availableBaseStations.Any())
            {
                throw new InvalidActionException("There is no available charge slot in any station.");
            }

            BaseStation closest = drone.FindClosest(availableBaseStations.Select(id => GetBaseStation(id)));

            if (!IsEnoughBattery(drone, closest.Location))
            {
                throw new InvalidActionException("Drone does not have enough battery to get to base station for charging.");
            }

            drone.Location = closest.Location;
            drone.State = DroneState.Maintenance;
            drone.Battery -= ElectricityConfumctiolFree * Localable.Distance(closest.Location, drone.Location);

            lock (Dal)
            {
                Dal.AddDroneCharge(drone.Id, closest.Id);
            }

            return closest.Id;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int FinishCharging(int droneId)
        {
            DroneForList drone = GetDroneForListRef(droneId);

            if (drone.State != DroneState.Maintenance)
            {
                throw new InvalidActionException("Drone is not being charged.");
            }
 
            var dalCharge = Dal.GetFilteredList<DO.DroneCharge>(c => c.DroneId == droneId).FirstOrDefault();
            if (dalCharge.Equals(default(DO.DroneCharge)))
            {
                throw new InvalidActionException("The drone is not charging");
            }

            drone.Battery = Math.Min(drone.Battery + ChargeRate * (DateTime.Now - dalCharge.StartTime).TotalHours,
                                     MAX_CHARGE);
            drone.State = DroneState.Free;
            
            lock (Dal)
            {
                Dal.DeleteWhere<DO.DroneCharge>(charge => charge.DroneId == drone.Id);
            }

            return dalCharge.StationId;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int AssignParcelToDrone(int droneId)
        {
            Drone drone = GetDrone(droneId);

            if (drone.State == DroneState.Deliver)
            {
                throw new InvalidActionException("Unable to associate a parcel with a busy drone");
            }

            var parcels = GetNotAssignedToDroneParcels().Select(parcel => GetParcelInDeliver(parcel.Id));

            var orderedParcels = from parcel in parcels
                                 where parcel.Weight <= drone.MaxWeight
                                       && IsAbleToDeliverParcel(drone, parcel)
                                 orderby parcel.Priority descending, parcel.Weight descending, Localable.Distance(GetCustomer(parcel.Sender.Id).Location, drone.Location)
                                 select parcel;

            if (!orderedParcels.Any())
            {
                throw new InvalidActionException("Unable to associate any parcel with the drone");
            }

            ParcelInDeliver selectedParcel = orderedParcels.First();

            lock (Dal)
            {
                Dal.Update<DO.Parcel>(selectedParcel.Id, nameof(DO.Parcel.DroneId), droneId);
                Dal.Update<DO.Parcel>(selectedParcel.Id, nameof(DO.Parcel.Scheduled), DateTime.Now);
            }

            var droneForList = GetDroneForListRef(droneId);
            droneForList.State = DroneState.Deliver;
            droneForList.DeliveredParcelId = selectedParcel.Id;

            return selectedParcel.Id;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int PickUpParcel(int droneId)
        {
            DroneForList drone = GetDroneForListRef(droneId);

            // Does the drone have a parcel?
            if (drone.DeliveredParcelId == null)
            {
                throw new InvalidActionException("No parcel is assigned to drone");
            }

            DO.Parcel parcel;
            try
            {
                parcel = Dal.GetById<DO.Parcel>((int)drone.DeliveredParcelId);
            }
            catch (DO.ObjectNotFoundException e)
            {
                throw new ObjectNotFoundException(typeof(Parcel), e);
            }

            // Was the parcel collected?
            if (parcel.PickedUp != null)
            {
                throw new InvalidActionException("The parcel associated with the drone has already been picked up");
            }

            ParcelInDeliver parcelInDeliver = GetParcelInDeliver(parcel.Id);

            lock (Dal)
            {
                Dal.Update<DO.Parcel>(parcel.Id, nameof(parcel.PickedUp), DateTime.Now);
            }

            drone.Battery -= Localable.Distance(drone.Location, parcelInDeliver.CollectLocation) * ElectricityConfumctiolFree;
            drone.Location = parcelInDeliver.CollectLocation;

            return parcel.Id;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int SupplyParcel(int droneId)
        {
            DroneForList drone = GetDroneForListRef(droneId);

            if (drone.DeliveredParcelId == null)
            {
                throw new InvalidActionException("No parcel is assigned to drone");
            }

            var parcel = GetParcel((int)drone.DeliveredParcelId);

            if (parcel.PickedUp == null)
            {
                throw new InvalidActionException("The parcel associated with the drone has not yet been picked up");
            }

            ParcelInDeliver parcelInDeliver = GetParcelInDeliver(parcel.Id);

            if (!parcelInDeliver.WasPickedUp)
            {
                throw new InvalidActionException("The parcel associated with the drone has already been supplied");
            }

            double neededBattery = Localable.Distance(drone.Location, parcelInDeliver.TargetLocation) * GetElectricity(parcelInDeliver.Weight);

            if (drone.Battery - neededBattery < 0)
            {
                throw new InvalidActionException("Drone does not have enough battery to get to target customer.");
            }

            drone.Battery -= neededBattery;
            drone.Location = parcelInDeliver.TargetLocation;
            drone.State = DroneState.Free;

            lock (Dal)
            {
                Dal.Update<DO.Parcel>(parcel.Id, nameof(parcel.Supplied), DateTime.Now);
            }

            return (int)drone.DeliveredParcelId;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DeleteDrone(int droneId)
        {
            Drone drone = GetDrone(droneId);

            if (drone.State != DroneState.Free)
                throw new InvalidActionException("Can not delete a non free drone");

            drones.RemoveAll(drone => drone.Id == droneId);

            lock (Dal)
            {
                Dal.Delete<DO.Drone>(droneId);
            }
        }

        #region Helpers

        /// <summary>
        /// Returns a refrence to a drone in the BL drones list
        /// </summary>
        /// <param name="id">The drone id</param>
        /// <returns>The drone refrence</returns>
        /// <exception cref="ObjectNotFoundException" />
        public DroneForList GetDroneForListRef(int id)
        {
            var drone = drones.FirstOrDefault(d => d.Id == id);

            if (drone == default)
            {
                throw new ObjectNotFoundException(typeof(DroneForList));
            }

            return drone;
        }

        /// <summary>
        /// Checks weather a drone can deliver a parcel
        /// </summary>
        /// <param name="drone">The deliver drone</param>
        /// <param name="parcel">The delivered parcel</param>
        /// <returns>true if the drone can deliver the parcel otherwise false</returns>
        private bool IsAbleToDeliverParcel(Drone drone, ParcelInDeliver parcel)
        {
            var neededBattery = Localable.Distance(drone.Location, parcel.CollectLocation) * ElectricityConfumctiolFree +
                                Localable.Distance(parcel.CollectLocation, parcel.TargetLocation) * GetElectricity(parcel.Weight) +
                                Localable.Distance(parcel.TargetLocation, parcel.TargetLocation.FindClosest(GetAvailableBaseStationsId().Select(id => GetBaseStation(id)))) * ElectricityConfumctiolFree;
            return drone.Battery >= neededBattery;
        }

        /// <summary>
        /// Checks weather a drone has enough battery to get to location
        /// </summary>
        /// <param name="drone">drone</param>
        /// <param name="location">location to get to</param>
        /// <returns>true if the drone can get the location otherwise false</returns>
        private bool IsEnoughBattery(DroneForList drone, Location location)
        {
            var neededBattery = Localable.Distance(drone.Location, location) * ElectricityConfumctiolFree;
            return drone.Battery >= neededBattery;
        }

        /// <summary>
        /// Returns a converted drone to drone in charge
        /// </summary>
        /// <param name="id">The drone Id</param>
        /// <returns>A <see cref="DroneInCharge"/></returns>
        internal DroneInCharge GetDroneInCharge(int id)
        {
            return new DroneInCharge()
            {
                Id = id,
                Battery = GetDroneForList(id).Battery,
            };
        }

        /// <summary>
        /// Returns a converted drone to drone in delivery
        /// </summary>
        /// <param name="id">The drone Id</param>
        /// <returns>A <see cref="DroneInDelivery"/></returns>
        internal DroneInDelivery GetDroneInDelivery(int id)
        {
            var drone = GetDrone(id);

            return new DroneInDelivery()
            {
                Id = id,
                Battery = drone.Battery,
                Location = drone.Location,
            };
        }

        #endregion
    }
}
