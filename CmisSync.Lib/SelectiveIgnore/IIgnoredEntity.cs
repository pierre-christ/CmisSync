//-----------------------------------------------------------------------
// <copyright file="IIgnoredEntity.cs" company="GRAU DATA AG">
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General private License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General private License for more details.
//
//   You should have received a copy of the GNU General private License
//   along with this program. If not, see http://www.gnu.org/licenses/.
//
// </copyright>
//-----------------------------------------------------------------------

namespace CmisSync.Lib.SelectiveIgnore
{
    using System;

    public abstract class IIgnoredEntity : IEquatable<IIgnoredEntity>
    {
        public string ObjectId { get; protected set; }
        public string LocalPath { get; protected set; }

        public bool Equals(IIgnoredEntity other) {
            return this.ObjectId == other.ObjectId;
        }
    }
}