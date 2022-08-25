using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct CharProperties
{
    public ulong health;
    public ulong maxHealth;
    public ulong mana;
    public ulong maxMana;
    public ulong energy;
    public ulong maxEnergy;

    public uint level;
    public ulong exp;
    public ulong expLevel; //Exp needed to be at this current level
    public ulong expTNL; //Exp till next level

    public CharProperties(ulong health, ulong maxHealth, ulong mana, ulong maxMana, ulong energy, ulong maxEnergy, uint level,
        ulong exp, ulong expLevel, ulong expTNL)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.mana = mana;
        this.maxMana = maxMana;
        this.energy = energy;
        this.maxEnergy = maxEnergy;
        this.level = level;
        this.exp = exp;
        this.expLevel = expLevel;
        this.expTNL = expTNL;
    }

    public CharProperties(InitPlayerVals packet)
    {
        this.health = packet.health;
        this.maxHealth = packet.maxHealth;
        this.mana = packet.mana;
        this.maxMana = packet.maxMana;
        this.energy = packet.energy;
        this.maxEnergy = packet.maxEnergy;
        this.level = packet.level;
        this.exp = packet.exp;
        this.expLevel = packet.expLevel;
        this.expTNL = packet.expTNL;
    }
}
