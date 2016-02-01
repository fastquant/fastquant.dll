import clr
import clrtype
clr.AddReference("SmartQuant")
clr.AddReference("System.Drawing")
from System import DateTime
from System.Drawing import Color
from SmartQuant import Scenario, InstrumentStrategy, Order, Group, CurrencyId
from SmartQuant import InstrumentManager, Instrument, BarType, Framework
from SmartQuant import DataObjectType, PositionSide
from SmartQuant.Indicators import BBL, BBU, SMA

class MyStrategy(InstrumentStrategy):
    __metaclass__ = clrtype.ClrClass

    AllocationPerInstrument = 100000
    K = 2
    Length = 10
    Qty = 100
    exitOrder = None
    bbu = None
    bbl = None
    sma = None
    bbu = None
    barsGroup = None
    fillGroup = None
    equityGroup = None
    bbuGroup = None
    bblGroup = None
    smaGroup= None

    def __new__(cls, *args):
        return InstrumentStrategy.__new__(cls, args[0], args[1])

    def OnFill(self, fill):
        self.Log(fill, self.fillGroup)

    def OnPositionOpened(self, position):
        self.UpdateExitLimit()


    def OnBar(self, instrument, bar):
        # Add bar to bar series.
        self.Bars.Add(bar)

        # Add bar to group.
        self.Log(bar, self.barsGroup)

        # Add upper bollinger band value to group.
        if self.bbu.Count > 0:
            self.Log(self.bbu.Last, self.bbuGroup)

        # Add lower bollinger band value to group.
        if self.bbl.Count > 0:
            self.Log(self.bbl.Last, self.bblGroup)

        # Add simple moving average value bands to group.
        if self.sma.Count > 0:
            self.Log(self.sma.Last, self.smaGroup)

        # Calculate performance.
        self.Portfolio.Performance.Update()

        # Add equity to group.
        self.Log(self.Portfolio.Value, self.equityGroup)

        # Check strategy logic.
        if not self.HasPosition(instrument):
            if self.bbu.Count > 0 and bar.Close >= self.bbu.Last:
                enterOrder = self.SellOrder(self.Instrument, self.Qty, "Enter")
                self.Send(enterOrder)
            else: 
                if self.bbl.Count > 0 and bar.Close <= self.bbl.Last:
                    enterOrder = self.BuyOrder(self.Instrument, self.Qty, "Enter")
                    self.Send(enterOrder)
        else:
            self.UpdateExitLimit()


    def OnStrategyStart(self):
        self.Portfolio.Account.Deposit(self.AllocationPerInstrument, CurrencyId.USD, "Initial allocation")
        self.bbu = BBU(self.Bars, self.Length, self.K)
        self.bbl = BBL(self.Bars, self.Length, self.K)
        self.sma = SMA(self.Bars, self.Length)
        self.AddGroups()

    def AddGroups(self):
        # Create bars group.
        self.barsGroup = Group("Bars")
        self.barsGroup.Add("Pad", DataObjectType.String, 0)
        self.barsGroup.Add("SelectorKey", Instrument.Symbol)

        # Create fills group.
        self.fillGroup = Group("Fills")
        self.fillGroup.Add("Pad", 0)
        self.fillGroup.Add("SelectorKey", Instrument.Symbol)

        # Create equity group.
        self.equityGroup = Group("Equity")
        self.equityGroup.Add("Pad", 1)
        self.equityGroup.Add("SelectorKey", Instrument.Symbol)

        # Create BBU group.
        self.bbuGroup = Group("BBU")
        self.bbuGroup.Add("Pad", 0)
        self.bbuGroup.Add("SelectorKey", Instrument.Symbol)
        self.bbuGroup.Add("Color", Color.Blue)

        # Create BBL group.
        self.bblGroup = Group("BBL")
        self.bblGroup.Add("Pad", 0)
        self.bblGroup.Add("SelectorKey", Instrument.Symbol)
        self.bblGroup.Add("Color", Color.Blue)

        # Create SMA group.
        self.smaGroup = Group("SMA")
        self.smaGroup.Add("Pad", 0)
        self.smaGroup.Add("SelectorKey", Instrument.Symbol)
        self.smaGroup.Add("Color", Color.Yellow)

        # Add groups to manager.
        self.GroupManager.Add(self.barsGroup)
        self.GroupManager.Add(self.fillGroup)
        self.GroupManager.Add(self.equityGroup)
        self.GroupManager.Add(self.bbuGroup)
        self.GroupManager.Add(self.bblGroup)
        self.GroupManager.Add(self.smaGroup)

    def UpdateExitLimit(self):
        if self.exitOrder != None and not self.exitOrder.IsDone:
            self.Cancel(self.exitOrder)

        if self.HasPosition(self.Instrument):
            if self.Position.Side == PositionSide.Long:
                self.exitOrder = self.SellLimitOrder(self.Instrument, self.Qty, self.sma.Last, "Exit")
            else:
                self.exitOrder = self.BuyLimitOrder(self.Instrument, self.Qty, self.sma.Last, "Exit")

            self.Send(self.exitOrder)

class Backtest(Scenario):
    __metaclass__ = clrtype.ClrClass

    barSize = 300

    def __new__(cls, *args):
        return Scenario.__new__(cls, args[0])

    def Run(self):
        instrument1 = self.InstrumentManager.Instruments["AAPL"]
        instrument2 = self.InstrumentManager.Instruments["MSFT"]

        self.strategy = MyStrategy(self.framework, "BollingerBands")

        self.strategy.AddInstrument(instrument1)
        self.strategy.AddInstrument(instrument2)

        self.DataSimulator.DateTime1 = DateTime(2013, 01, 01)
        self.DataSimulator.DateTime2 = DateTime(2013, 12, 31)

        self.BarFactory.Add(instrument1, BarType.Time, self.barSize)
        self.BarFactory.Add(instrument2, BarType.Time, self.barSize)

        self.StartStrategy()

if __name__ == '__main__':
    f = Framework("IronPython")
    print(f.Name)
    scenario = Backtest(f)
    scenario.Run()
