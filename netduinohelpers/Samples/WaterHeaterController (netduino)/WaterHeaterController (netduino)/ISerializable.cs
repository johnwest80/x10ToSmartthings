using System;
using Microsoft.SPOT;
using System.Collections;

namespace WaterHeaterController {
    public interface ISerializable {
        void Serialize(Serializer serial);
        void Deserialize(Deserializer serial);
        void Display(ArrayList displayList);
    }
}
