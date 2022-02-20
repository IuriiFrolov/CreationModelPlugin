using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CreationModelPlugin
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {// Лекция № 4 Создание модели  важная статья https://digitteck.com/dotnet/families-symbols-instances-and-system-families/
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) // добирается до документа, сообщение при неудачи, набор элементов
        {
            Document doc = commandData.Application.ActiveUIDocument.Document; 

            Level level1 = GetLevelByName(doc, "Уровень 1"); // используем метод для нахождения уровня
            Level level2 = GetLevelByName(doc, "Уровень 2"); // используем метод для нахождения уровня

            List<Wall> walls = CreateWall(doc, level1, level2, 10000, 5000); // используем метод для создания стен и добавления их в список (документ, низ стены, верх стены, Длина, Ширина)

            return Result.Succeeded;
        }
        
        // метод для нахождения уровня
        public Level GetLevelByName( Document doc, string nameLevel)
        {
            Level level = new FilteredElementCollector(doc)
                       .OfClass(typeof(Level))
                       .OfType<Level>()
                       .Where(x => x.Name.Equals(nameLevel))
                       .FirstOrDefault();

            return level;
        }

        // Вспомогательный метод для построения опорных точек, передается в CreateWall 
        public List<XYZ> CreatePointsWall(Document doc, double widthInMilimeters, double depthInMilimeters)
        {
            double oWidth = UnitUtils.ConvertToInternalUnits(widthInMilimeters, UnitTypeId.Millimeters);
            double oDepth = UnitUtils.ConvertToInternalUnits(depthInMilimeters, UnitTypeId.Millimeters);
            double dx = oWidth / 2;
            double dy = oDepth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            return points;
        }

        // Основной метод для создания 4-х стен и добавления стен в лист.
        public List<Wall> CreateWall (Document doc, Level level1, Level level2, double widthInMilimeters, double depthInMilimeters)
        {
            List<XYZ> points = CreatePointsWall(doc, widthInMilimeters, depthInMilimeters);

            List<Wall> oWalls = new List<Wall>();
            Transaction transaction = new Transaction(doc, "Построение стен");
            transaction.Start();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]); // Ось стены из 2 точек
                Wall wall = Wall.Create(doc, line, level1.Id, false); // Создаем стену (в активном документе, по линии заданной 2 точками, на 1 уровне низ стены, НЕ НЕСУЩАЯ)
                oWalls.Add(wall); // Добавляем стену в лист (список) "Стены"
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(level2.Id);  // устанавливаем высоту стены до 2 уровня
            }
            transaction.Commit();
            return oWalls;

        }


    }
}



//  var res1= new FilteredElementCollector(doc) /* .OfType - фильтр LINQ (в подсказке написано IEnumerable) !!!,
//                                               * а .OfCategory & .OfCategoryId &  .OfClass  напротив принадлежат Revit (в подсказке написано FilteredElementCollector),
//                                               * необходимо внимательно читать подсказки */
//                .OfClass(typeof(WallType))  /*typeof объектно орентированное представление типа, т.е не человек а его страница в FaceBook */
//                //.Cast<Wall>()  /*метод расширения которые выполняет ПРЕОБРАЗОВАНИЕ каждого элемента в списке к заданному типу <Wall>  не безопасное привидение может выдать исключение*/
//                .OfType<WallType>()   /* метод расширения которые выполняет ФИЛЬТРАЦИЮ на основе заданного типа, безопасное привидение к списку стен, LINQ  работает медленно, ставим в конце*/
//                .ToList();

//var res2 = new FilteredElementCollector(doc)
//    .OfClass(typeof(FamilyInstance))
//    .OfCategory(BuiltInCategory.OST_Doors) // фильтруем все двери
//    .OfType<FamilyInstance>()
//    .Where(x => x.Name.Equals("36 x 84"))   // поиск по имени двери
//    .ToList();

//var res3 = new FilteredElementCollector(doc)
//    //.WhereElementIsElementType() // быстрый фильтр отбирает то что относится к типаразмерам
//    .WhereElementIsNotElementType() // находит экземпляры, т.е конкретно двери, окна ....
//    .ToList();



