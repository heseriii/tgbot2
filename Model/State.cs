using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public enum State
    {
        Idle, // Состояние ожидания команды
        AwaitingPassword,
        AwaitingTags, // Ожидание ввода тегов
        AwaitingDescription // Ожидание ввода описания
    }
}
