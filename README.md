# RevitExportGeometryToAutocad
Вспомогательные библиотеки для отрисовки геометрии из Revit в AutoCAD в виде простых объектов (отрезок, дуга, точка) посредством экспорта в xml
## Описание
Библиотеки пригодятся при разработке плагинов, связанных с геометрией, для удобного визуального восприятия результатов. На мой взгляд просматривать результат в AutoCAD намного удобнее.

В данном проекте присутствует две библиотеки (одна для Revit, вторая для AutoCAD) и демо-проект для Revit.
## Версионность
Проект для AutoCAD собран с использованием библиотек от AutoCAD 2013. Будет работать со всеми последующими версиями AutoCAD

Проект Revit собран с использованием библиотек от Revit 2015. Должен работать со всеми последующими версиями (с 2015-2018 точно работает)

Библиотека **RevitGeometryExporter** собирается под `net45` и `net48` и распространяется как NuGet-пакет.

## Использование
Решение также содержит демо-проект для Revit. Описание использования на примере этого проекта:

**В Revit**

RevitGeometryExporter нужна только на этапе разработки и отладки. Библиотека устроена так, что в Debug она работает и копируется в выходную папку, а в Release вызовы её методов удаляются компилятором и сама dll в выходную папку не попадает. Никаких `#if DEBUG` и путей к dll в проекте не требуется.

### Как это работает
* Все публичные методы `ExportGeometryToXml` помечены атрибутом `[Conditional("DEBUG")]` – как `Debug.Print`. Если в проекте, который **вызывает** метод, не определена константа `DEBUG`, компилятор удаляет вызов целиком (включая вычисление аргументов). В итоговой сборке плагина не остаётся даже ссылки на RevitGeometryExporter.
* В пакет вложен файл `build\RevitGeometryExporter.targets`, который NuGet автоматически подключает к проекту. Если в `DefineConstants` проекта нет `DEBUG`, этот target удаляет `RevitGeometryExporter.dll` (и .pdb/.xml) из списка копируемых в выходную папку файлов.
* Решение принимается по константе `DEBUG`, а не по имени конфигурации, поэтому работает и с пользовательскими конфигурациями (например, под версии Revit) – важно лишь, определён ли в них `DEBUG`.

Условия на `PackageReference` по `$(Configuration)` для этой задачи не подходят: restore NuGet выполняется один раз для всех конфигураций, а копирование в выходную папку решается уже на этапе сборки – там и сделано исключение.

### Подключение
Пакет подключается во всех конфигурациях (чтобы код компилировался):
```xml
<ItemGroup>
  <PackageReference Include="RevitGeometryExporter" Version="1.3.0" PrivateAssets="all" />
</ItemGroup>
```
`PrivateAssets="all"` – чтобы пакет не превращался в транзитивную зависимость проектов, которые ссылаются на ваш.

**Проекты под .NET Framework (net45 – net48)** – ничего дополнительно не требуется.

**Проекты под net8.0 / net10.0 (Revit 2025+)**:
* В SDK-проектах типа «библиотека» под современный .NET по умолчанию dll из NuGet-пакетов **не копируются** в выходную папку (`CopyLocalLockFileAssemblies = false`) – предполагается, что их подтянет приложение. Для плагина Revit это не так, поэтому в проекте должно быть одно из:
```xml
<PropertyGroup>
  <!-- рекомендуемый вариант для плагинов: включает копирование зависимостей и создаёт runtimeconfig.json -->
  <EnableDynamicLoading>true</EnableDynamicLoading>
  <!-- или только копирование зависимостей -->
  <!-- <CopyLocalLockFileAssemblies>true</CopyLocalLockFileAssemblies> -->
</PropertyGroup>
```
Если в выходную папку проекта уже попадают dll других NuGet-пакетов, значит одно из свойств уже задано и дописывать ничего не нужно.
* Пакет содержит сборки только под .NET Framework, поэтому при restore появится предупреждение **NU1701** (используется сборка `net48`). Это ожидаемо, его можно отключить:
```xml
<PackageReference Include="RevitGeometryExporter" Version="1.3.0" PrivateAssets="all" NoWarn="NU1701" />
```
* Именно поэтому в пакете есть сборка `net48`: для net8.0/net10.0 NuGet умеет подставлять только сборки net461 – net481, а `net45` в этот список не входит. По той же причине targets-файл лежит в `build\net45` и `build\net48`, а не в корне `build`: если бы в пакете был TFM-независимый `build\*.targets`, NuGet считал бы пакет совместимым с net8.0, не применял бы подстановку net48 и молча не подключал бы саму dll.

### Использование в коде
* Инициализация – папка для экспорта xml и (при необходимости) единицы вывода:
```csharp
// setup export folder
ExportGeometryToXml.Init(@"C:\Temp");

// setup export folder and units (feet or millimeters)
ExportGeometryToXml.Init(@"C:\Temp", ExportUnits.Mm);

// setup export folder, units and clear folder
ExportGeometryToXml.Init(@"C:\Temp", ExportUnits.Mm, true);
```
По умолчанию в библиотеке прописан путь *C:\Temp\RevitExportXml*, единицы – футы. В случае отсутствия директории она будет создана.

> **Важно!** Свойства `ExportGeometryToXml.FolderName` и `ExportGeometryToXml.ExportUnits` доступны только для чтения – задаются они только через методы `Init(...)`. Читать их в коде плагина также не следует: атрибут `[Conditional]` действует только на методы, поэтому обращение к свойству останется в Release-сборке, а dll в выходной папке не будет – плагин упадёт во время работы.

*	Вызвать один или несколько методов экспорта геометрии. 
Например:
```csharp
List<Wall> wallsToExport = new List<Wall>();
foreach (Reference reference in selectionResult)
{
    Wall wall = (Wall)doc.GetElement(reference);
    wallsToExport.Add(wall);
}
ExportGeometryToXml.ExportWallsByFaces(wallsToExport, "walls");
```
Или
```csharp
List<FamilyInstance> familyInstances = new List<FamilyInstance>();
foreach (Reference reference in selectionResult)
{
    Element el = doc.GetElement(reference);
    if(el is FamilyInstance familyInstance)
        familyInstances.Add(familyInstance);
}
ExportGeometryToXml.ExportFamilyInstancesByFaces(familyInstances, "families", false);
```
Директивы `#if DEBUG` вокруг этих вызовов не нужны.

### Сборка пакета
* Пакет создаётся автоматически при сборке проекта RevitGeometryExporter в конфигурации **Release** (`GeneratePackageOnBuild`) и кладётся в `RevitGeometryExporter\bin\nupkg`.
* Версия пакета задаётся свойством `<Version>` в `RevitGeometryExporter.csproj`, версия сборки – в `Properties\AssemblyInfo.cs`.
* Сборка самой библиотеки в Release не влияет на поведение: удаление вызовов зависит от константы `DEBUG` проекта, который использует библиотеку.

**В AutoCAD**
*	С помощью команды **NETLOAD** загрузить библиотеку **CadDrawGeometry.dll**.
* Использовать одну из двух доступных команд:

**DrawFromOneXml** – отрисовка геометрии из одного указанного xml-файла

**DrawFromSeveralXml** – отрисовка геометрии из нескольких указанных xml-файлов. По аналогии с DrawFromOneXml, только в окне выбор файлов включена возможность мультивыбора (через Shift или Ctrl)

**DrawXmlFromFolder** - отрисовка геометрии из указанной папки в который должны располагаться xml-файлы

## Пример
Элементы в Revit:

<img alt="Screenshot_1" src="./docs/Screenshot_1.png">

Результат экспорта и отрисовки геометрии в AutoCAD:

<img alt="Screenshot_2" src="./docs/Screenshot_2.png">
