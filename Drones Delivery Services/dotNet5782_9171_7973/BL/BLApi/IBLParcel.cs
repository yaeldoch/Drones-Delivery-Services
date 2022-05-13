﻿using BO;
using System.Collections.Generic;

namespace BLApi
{
    /// <summary>
    /// Declares all <see cref="IBL"/> methods related to <see cref="Parcel"/>
    /// </summary>
    public interface IBLParcel
    {
        /// <summary>
        /// Adds a parcel
        /// </summary>
        /// <param name="senderId">the parcel sender customer id</param>
        /// <param name="targetId">the parcel target customer id</param>
        /// <param name="weight">the parcel weight</param>
        /// <param name="priority">the parcel priority</param>
        /// <exception cref="IdAlreadyExistsException" />
        /// <exception cref="InvalidPropertyValueException" />
        int AddParcel(int senderId, int targetId, WeightCategory weight, Priority priority);

        /// <summary>
        /// Returns a specific parcel
        /// </summary>
        /// <param name="id">id of requested parcel</param>
        /// <returns>parcel with id</returns>
        /// <exception cref="ObjectNotFoundException" />
        Parcel GetParcel(int id);

        /// <summary>
        /// Returns a converted parcel to parcel for list
        /// </summary>
        /// <param name="id">The parcel id</param>
        /// <returns>A <see cref="ParcelForList"/></returns>
        /// <exception cref="ObjectNotFoundException" />
        ParcelForList GetParcelForList(int id);

        /// <summary>
        /// Returns the parcels list
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> parcels list</returns>
        IEnumerable<ParcelForList> GetParcelsList();

        /// <summary>
        /// Returns a list of parcels which weren't assigned to drone yet
        /// </summary>
        /// <returns>list of parcels which weren't assigned to drone yet</returns>
        IEnumerable<ParcelForList> GetNotAssignedToDroneParcels();

        /// <summary>
        /// Deletes a parcel
        /// </summary>
        /// <param name="parcelId">The parcel Id</param>
        /// <exception cref="ObjectNotFoundException"></exception>
        /// <exception cref="InvalidActionException" />
        void DeleteParcel(int parcelId);
    }
}
