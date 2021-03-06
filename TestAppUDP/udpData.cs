﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;


namespace TestAppUDP
{
    /// <summary>
    /// Принимаемые данные по UDP
    /// </summary>
    [Table("udpData")]
    [Serializable]
    public class UdpData
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();
        [NotMapped]
        public long count { get; set; } = 0;
        [Column("lostPackages")]
        public long lostPackages { get; set; } = 0;
        [Column("dateTime")]
        public DateTime dateTime { get; set; }
        [Column("value1")]
        public int value1 { get; set; } = 0;
        [Column("value2")]
        public int value2 { get; set; } = 0;
        [Column("value3")]
        public int value3 { get; set; } = 0;
        [Column("value4")]
        public int value4 { get; set; } = 0;
        [Column("value5")]
        public int value5 { get; set; } = 0;
        [NotMapped]
        public bool HasValue { get; set; } = false;

        public UdpData()
        {
            this.dateTime = DateTime.Now;
        }
        public UdpData(long count, int value1, int value2, int value3, int value4, int value5)
        {
            this.dateTime = DateTime.Now;
            this.count = count;
            this.value1 = value1;
            this.value2 = value2;
            this.value3 = value3;
            this.value4 = value4;
            this.value5 = value5;
            this.HasValue = true;
        }
        public override string ToString()
        {
            return count.ToString()
                + "  " + lostPackages.ToString()
                + "  " + value1.ToString()
                + "  " + value2.ToString()
                + "  " + value3.ToString()
                + "  " + value4.ToString()
                + "  " + value5.ToString();
        }
    }
}
