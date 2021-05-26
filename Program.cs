using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Задание_2_B_1336
{

    interface IDataFileReader
    {
        string ReadDataFromFile(string fileName);
    }
    interface IDataFileWritter
    {
        void WriteDataToFile<T>(string fileName, List<T> data);
    }
    /// <summary>
    /// Интерфейс парсера
    /// </summary>
    interface IParser
    {
        List<T> DataParsing<T>(string rawString);
    }

 
    /// <summary>
    /// Для парсинга данных из текстогового файла
    /// </summary>
    class JsonReadeerWritter : IDataFileReader, IDataFileWritter, IParser
    {
        /// <summary>
        /// Возвращает строку, заполненную текстовым файлом
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ReadDataFromFile(string fileName)
        {
            try
            {
                string rawJsonString = File.ReadAllText(fileName);
                return rawJsonString;
            }
            catch (Exception error )
            {
                Console.WriteLine(error.Message);
                return null;
            }
        }
        public void WriteDataToFile<T>(string fileName, List<T> data)
        {            
            try
            {
                string jsonString = JsonSerializer.Serialize(data);
                File.WriteAllText(fileName, jsonString);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
            }
            
        }
        /// <summary>
        /// Реализация интерфейса IParser для данных в формате JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rawString"> </param>
        /// <returns></returns>
        public List<T> DataParsing<T>(string rawString)
        {
            try
            {
                List<T> deviceList = JsonSerializer.Deserialize<List<T>>(rawString);
                return deviceList;
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return null;
            }
        }
    }

    /// <summary>
    /// Класс для решения технического задания
    /// </summary>
    class BrigadesGroups
    {
        /// <summary>
        /// Инкапсулированные и сгруппированные данные
        /// </summary>
        public IEnumerable<IGrouping<string, Device>> groups;
        /// <summary>
        /// Конструктор класса  BrigadesGroups во время инициализации группирует элементы по коду бригады
        /// </summary>
        /// <param name="deviceList"></param>
        public BrigadesGroups(List<DeviceInfo> deviceList)
        {
            try
            {
                groups = from item in deviceList
                         //Группируем элементы по коду бригады
                         group item.device by item.brigade.code;
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                groups = null;
            }
        }
        /// <summary>
        /// Метод исключающий групировки с всеми выключенными приборами
        /// </summary>
        public void DeccludeNonOnlineBrigades()

        {

            // Отсортировываем только с тех у которых есть прибор онлайн
            groups = from item in groups
                     where item.Any<Device>(d => d.isOnline == true)
                     select item;
        }
        /// <summary>
        /// Формирования списка Conflict из группировок 
        /// </summary>
        /// <returns></returns>
        public List<Conflict> CreateConflictsList()
        {
            List<Conflict> result = new List<Conflict>();
            foreach (IGrouping<string, Device> brigadeGroup in groups)
            {
                // вытаскиваем key
                string brigadeCode = brigadeGroup.Key;
                // создаем списки номеров из списков девайсов
                
                List<string> devicesSerials = new List<string>();
                foreach (Device device in brigadeGroup.ToList<Device>())
                {
                    devicesSerials.Add(device.serialNumber);
                }
                result.Add(new Conflict { BrigadeCode = brigadeCode, DevicesSerials = devicesSerials.ToArray() });

            }
            return result;

        }
    }
    

    class Program
    {
        static void Main(string[] args)
        {

            JsonReadeerWritter jsoner = new JsonReadeerWritter();
            // Чтение данных из файла
            string rawData = jsoner.ReadDataFromFile("D:\\Pet projects\\Задание-2-B-1336\\Data\\Devices.json");
            // Преобразование в класс DeviceInfo - в дальнешем в зависимости от  поступаеммых данных может не требоваться
            List<DeviceInfo> rawInfo = jsoner.DataParsing<DeviceInfo>(rawData);
            // Группирирование бригад по номеры
            BrigadesGroups brg = new BrigadesGroups(rawInfo);
            // Исколючение не работающих
            brg.DeccludeNonOnlineBrigades();
            // Формировани нового списка
            List<Conflict> conflicts = brg.CreateConflictsList();
            // Запись  данных в файл
            // скорее всего здесь допущенна ошибка, т.к. используется конкретная реализация, что уменьшает гибкость !
            jsoner.WriteDataToFile("resultFile.txt", conflicts);

        }
    }
}
 