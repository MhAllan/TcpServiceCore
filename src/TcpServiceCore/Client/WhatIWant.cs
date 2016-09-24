using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlanCF.Client
{
    public class WhatIWant
    {
        public static void init()
        {
            Gen g = new Gen(new Chnl());
        }
    }

    public interface IClienChannel
    {
        void Open();
        void Close(string msg);
    }

   public interface ISer
    {
        Task<string> Go(string msg, object o1, object o2, object o3, object o4, object o5,
            object o6, object o7, object o8, object o9);
    }

   public class Gen : IClienChannel, ISer
    {
        Chnl channel;
        public Gen(Chnl channel)
        {
            this.channel = channel;
        }
        public void Close(string msg)
        {
            this.channel.Close(msg);
        }

        public Task<string> Go(string msg, object o1, object o2, object o3, object o4, object o5,
            object o6, object o7, object o8, object o9)
        {
            return this.channel.Go("go", msg, o1, o2, o3, o4, o5, o6, o7, o8, o9);
        }

        public void Open()
        {
            this.channel.Open();
        }
    }

   public class Chnl : IClienChannel
    {
        public void Close(string msg)
        {
            //
        }

        public void Open()
        {
            //
        }

        public Task<string> Go(string method, params object[] obj)
        {
            //
            return Task.FromResult("hello");
        }
    }


}
