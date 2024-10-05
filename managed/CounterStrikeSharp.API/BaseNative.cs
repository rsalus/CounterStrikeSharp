/*
 *  This file is part of CounterStrikeSharp.
 *  CounterStrikeSharp is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  CounterStrikeSharp is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with CounterStrikeSharp.  If not, see <https://www.gnu.org/licenses/>. *
 */

using System.Reflection;

namespace CounterStrikeSharp.API
{
    /// <summary>
    /// Represents a base class for objects that wrap native (unmanaged) resources.
    /// </summary>
    public abstract class NativeObject
    {
        /// <summary>
        /// Gets the native handle associated with this object.
        /// </summary>
        public IntPtr Handle { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeObject"/> class.
        /// </summary>
        /// <param name="pointer">The native handle to associate with this object.</param>
        protected NativeObject(IntPtr pointer)
        {
            Handle = pointer;
        }

        /// <summary>
        /// Creates a new instance of the specified type <typeparamref name="T"/>,
        /// using the current object's handle to initialize the new instance.
        /// </summary>
        /// <remarks>
        /// Useful for creating a new instance of a class that inherits from 
        /// <see cref="NativeObject"/> and shares the same native handle.
        /// For example: 
        /// <code>
        /// var weaponServices = playerWeaponServices.As&lt;CCSPlayer_WeaponServices&gt;();
        /// </code>
        /// </remarks>
        /// <typeparam name="T">The type to create. Must inherit from <see cref="NativeObject"/>.</typeparam>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        public T As<T>() where T : NativeObject
        {
            // Check if handle is null
            if (Handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Cannot create a derived instance with a null or invalid handle.");
            }

            // Return instance with binding flags to ensure expected behavior
            return (T)Activator.CreateInstance(typeof(T),
                BindingFlags.NonPublic |
                BindingFlags.Instance, null, [Handle], null
            )!;
        }
    }
}
