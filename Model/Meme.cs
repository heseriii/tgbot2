﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Meme: IDomainObject
    {
        public int Id { get; set; }
        public string ImageId { get; set; }
        public string Tags { get; set; }
        public string Description { get; set; }
        public State State { get; set; } = State.Idle;
    }
}
