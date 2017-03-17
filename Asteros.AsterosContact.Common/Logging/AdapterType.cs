namespace Asteros.AsterosContact.Common.Logging
{
    /// <summary>
    /// Тип адаптера.
    /// </summary>
    // TODO: Пока я не понимаю, зачем нужно данное перечисление. Необходимо это выяснить.
    // TODO: После того, как будет выяснено назначение данного перечисления, внести поясняющий
    // TODO: комментарий.
    public enum AdapterType
    {
        // Файл.
        File = 0,
        // База данных.
        Database = 1,
        // Служба Windows Communication Foundation.
        Wcf = 2,
        // Логирование события.
        EventLog = 3
    }
}