using Core;
using Newtonsoft.Json;
using System.Reflection;

namespace DAL.Core
{
    public static class TestDataHelper
    {
      public  static IQueryable<T> GetFilledEntities<T>(int count) where T : class, IEntity
        {
            List<T> entityList = new List<T>();
            var fileName = typeof(T).Name+".json";
            //if (File.Exists(fileName))
            //{
            // entityList = JsonConvert.DeserializeObject<List<T>>(fileName);
            //}
            //else
            {
                for (int i = 1; i <= count; i++)
                {
                    T entity = Activator.CreateInstance<T>();
                    entity.Id = i;
                    FillRandomValues(entity);
                    entityList.Add(entity);
                }
//                File.WriteAllText(fileName, JsonConvert.SerializeObject(entityList));
            }
            IQueryable<T> queryableEntities = entityList.AsQueryable();
            return queryableEntities;
        }

      public  static void FillRandomValues<T>(T entity) where T : class
        {
            // Получение всех полей класса T
            PropertyInfo[] fields = typeof(T).GetProperties();

            // Заполнение полей случайными значениями
            Random random = new Random();
            foreach (PropertyInfo field in fields)
            {
                if (field.Name == "Id") continue;
                var ft = field.PropertyType;
                if (ft.IsNullable())
                    ft = Nullable.GetUnderlyingType(field.PropertyType);


                if ( ft== typeof(int))
                {
                    field.SetValue(entity, random.Next());
                }
                else if (ft == typeof(string))
                {
                    field.SetValue(entity, Guid.NewGuid().ToString());
                }
                else if (ft == typeof(DateTime))
                {
                    DateTime currentDate = DateTime.Now;

                    // Получение случайного числа от -365 до 365 (год в днях)
                    int randomDays = new Random().Next(-365, 366);

                    // Получение случайной даты в диапазоне плюс-минус год
                    DateTime randomDate = currentDate.AddDays(randomDays);
                    field.SetValue(entity,randomDate );
                }
                else if (ft == typeof(bool))
                {
                    // Заполнение булевого значения случайным образом
                    field.SetValue(entity, random.Next(2) == 0);
                }

            }
        }
   
    }


}
