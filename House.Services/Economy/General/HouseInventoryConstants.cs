using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace House.House.Services.Economy.General;

public static class HouseInventoryConstants
{
    public static class General
    {
        public const int MaxItems = 32_767;
    }

    public static class Knives
    {
        public const int Max = 5;
    }

    public static class Food
    {
        public const int Max = 1_500;
    }

    public static class Stimulant
    {
        public const int Max = 1_250;
    }

    public static class Backpack
    {
        public const int Max = 15;
        public const int PerBackpack = 350;
    }
}