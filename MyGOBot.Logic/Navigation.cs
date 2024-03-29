﻿using MyGOBot.Logic.Utils;
using POGOProtos.Networking.Responses;
using PokemonGo.RocketAPI;
using System;
using System.Device.Location;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace MyGOBot.Logic {
	
	public class Navigation {
		private const double SpeedDownTo = 10 / 3.6;
		private readonly Client _client;

		public Navigation(Client client) {
			_client = client;
		}

		public async Task<PlayerUpdateResponse> Move(GeoCoordinate targetLocation,
			double walkingSpeedInKilometersPerHour, Func<Task> functionExecutedWhileWalking,
			CancellationToken cancellationToken, bool disableHumanLikeWalking) {
			cancellationToken.ThrowIfCancellationRequested();

			if (!disableHumanLikeWalking) {
				var speedInMetersPerSecond = walkingSpeedInKilometersPerHour / 3.6;

				var sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
				LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);
				// Logger.Write($"Distance to target location: {distanceToTarget:0.##} meters. Will take {distanceToTarget/speedInMetersPerSecond:0.##} seconds!", LogLevel.Info);

				var nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
				var nextWaypointDistance = speedInMetersPerSecond;
				var waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);

				//Initial walking
				var requestSendDateTime = DateTime.Now;
				var result =
					await
						_client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude,
							_client.Settings.DefaultAltitude);
				
				do {
					cancellationToken.ThrowIfCancellationRequested();

					var millisecondsUntilGetUpdatePlayerLocationResponse =
						(DateTime.Now - requestSendDateTime).TotalMilliseconds;

					sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
					var currentDistanceToTarget = LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);

					if (currentDistanceToTarget < 40) {
						if (speedInMetersPerSecond > SpeedDownTo) {
							//Logger.Write("We are within 40 meters of the target. Speeding down to 10 km/h to not pass the target.", LogLevel.Info);
							speedInMetersPerSecond = SpeedDownTo;
						}
					}

					nextWaypointDistance = Math.Min(currentDistanceToTarget,
						millisecondsUntilGetUpdatePlayerLocationResponse / 1000 * speedInMetersPerSecond);
					nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
					waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);

					requestSendDateTime = DateTime.Now;
					result =
						await
							_client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude,
								_client.Settings.DefaultAltitude);
					
					if (functionExecutedWhileWalking != null)
						await functionExecutedWhileWalking(); // look for pokemon
				} while (LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation) >= 30);

				return result;
			} else {
				var result =
						await
							_client.Player.UpdatePlayerLocation(targetLocation.Latitude, targetLocation.Longitude,
								_client.Settings.DefaultAltitude);
				
				return result;
			}

		}

		public async Task<PlayerUpdateResponse> HumanPathWalking(GpxReader.Trkpt trk,
			double walkingSpeedInKilometersPerHour, Func<Task> functionExecutedWhileWalking,
			CancellationToken cancellationToken) {
			cancellationToken.ThrowIfCancellationRequested();

			//PlayerUpdateResponse result = null;

			var targetLocation = new GeoCoordinate(Convert.ToDouble(trk.Lat, CultureInfo.InvariantCulture),
				Convert.ToDouble(trk.Lon, CultureInfo.InvariantCulture));

			var speedInMetersPerSecond = walkingSpeedInKilometersPerHour / 3.6;

			var sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
			LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);
			// Logger.Write($"Distance to target location: {distanceToTarget:0.##} meters. Will take {distanceToTarget/speedInMetersPerSecond:0.##} seconds!", LogLevel.Info);

			var nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
			var nextWaypointDistance = speedInMetersPerSecond;
			var waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing,
				Convert.ToDouble(trk.Ele, CultureInfo.InvariantCulture));

			//Initial walking

			var requestSendDateTime = DateTime.Now;
			var result =
				await
					_client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude, waypoint.Altitude);
			
			do {
				cancellationToken.ThrowIfCancellationRequested();

				var millisecondsUntilGetUpdatePlayerLocationResponse =
					(DateTime.Now - requestSendDateTime).TotalMilliseconds;

				sourceLocation = new GeoCoordinate(_client.CurrentLatitude, _client.CurrentLongitude);
				var currentDistanceToTarget = LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation);

				//if (currentDistanceToTarget < 40)
				//{
				//    if (speedInMetersPerSecond > SpeedDownTo)
				//    {
				//        //Logger.Write("We are within 40 meters of the target. Speeding down to 10 km/h to not pass the target.", LogLevel.Info);
				//        speedInMetersPerSecond = SpeedDownTo;
				//    }
				//}

				nextWaypointDistance = Math.Min(currentDistanceToTarget,
					millisecondsUntilGetUpdatePlayerLocationResponse / 1000 * speedInMetersPerSecond);
				nextWaypointBearing = LocationUtils.DegreeBearing(sourceLocation, targetLocation);
				waypoint = LocationUtils.CreateWaypoint(sourceLocation, nextWaypointDistance, nextWaypointBearing);

				requestSendDateTime = DateTime.Now;
				result =
					await
						_client.Player.UpdatePlayerLocation(waypoint.Latitude, waypoint.Longitude,
							waypoint.Altitude);
				
				if (functionExecutedWhileWalking != null)
					await functionExecutedWhileWalking(); // look for pokemon & hit stops

			} while (LocationUtils.CalculateDistanceInMeters(sourceLocation, targetLocation) >= 30);

			return result;
		}

	}

}
