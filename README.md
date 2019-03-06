# StockExchange

## Opis problemu

Projekt jest symulacją zachowania graczy na giełdzie oraz systemu giełdowego. Maklerzy okresowo składają zlecenia na zakup lub sprzedaż pewnej liczby akcji po określonej cenie. System giełdowy znajduje pary komplementarnych zleceń. Zlecenia są ze sobą komplementarne jeśli dotyczą tej samej spółki giełdowej oraz jedno z nich jest zleceniem sprzedaży, a drugie zleceniem zakupu. Ponadto cena maksymalna, którą oferuje kupujący jest większa od ceny minimalnej, która deklaruje sprzedający.

Niepożądane jest kilkukrotne sprzedanie lub kupno tych samych akcji.

Dla uproszczenia przyjmujemy, że maklerzy nie modyfikują zlecenia po jego złożeniu.


## Zaproponowane rozwiązanie

Okresowo każdy automat giełdowy próbuje przeprowadzić transakcję kupna-sprzedaży akcji dla losowej spółki giełdowej. W tym celu wykonuje następujący algorytm:
1. Pobierz wszystkie zlecenia dla danej spółki nie zablokowane w danym momencie przez żaden inny automat. To jest zlecenia o pustym zbiorze `LockedBy` (patrz [Schema](#schema)). (*ONE*)
2. Znajdź parę zleceń komplementarnych.
3. Zablokuj zlecenia znalezione w punkcie 2. - dodaj identyfikator tego automatu do zbioru `LockedBy` zleceń. (QUORUM)
4. Sprawdź czy zlecenia są zablokowane jedynie przez ten automat. Jeżeli nie wycofaj się - usuń identyfikator tego automatu ze zbioru `LockedBy` zleceń. (*QUORUM*)
5. Przeprowadź transakcję wykonując operacje wsadowo: (*QUORUM*)
    - Usuń zlecenia.
    - Jeśli któreś z usuwanych zleceń nie zostało w pełni zaspokojone, utwórz odpowiadający rekord zlecenia z odpowiednio zmniejszoną liczbą akcji.
    - Utwórz rekord transakcji.

Według powyższego algorytmu wiele automatów, próbujących zająć te same akcje, nie powinno przejść jednocześnie do punktu 5.
Ponieważ przy dodawaniu i usuwaniu swoich identyfikatorów do zbiorów `LockedBy` wykorzystywana jest poziom spójności *QUORUM*, to co najwyżej jeden z automatów zobaczy w kroku 4. jedynie *swój* identyfikator w zbiorze `LockedBy`.
Wszystkie inne automaty zobaczą w zbiorze `LockedBy` wiele identyfikatorów i wycofają się.

Dopuszczana jest sytuacja, że wszystkie automaty spróbują zablokować to samo zlecenie i obydwa się wycofają.
Preferujemy bezpieczeństwo ponad przepustowość.


## Schema

### Zlecenia kupna/sprzedaży

``` csharp
OrderId uuid          // identyfikator zlecenia
StockSymbol text      // symbol spółki giełdowej, której akcji dotyczy zlecenie
SubmitterId uuid      // identyfikator zlecającego
SubmitterName text    // imię i nazwisko zlecającego
Quantity int          // liczba akcji, które należy kupić/sprzedać w ramach zlecenia
OrderType int         // rodzaj zlecenia (0 - sprzedaż, 1 - zakup)
PricePerUnit decimal  // cena za jednostkę akcji (minimalna w przypadku sprzedaży, maksymalna w przypadku zakupu)
Date timestamp        // data i czas powstania zlecenia 
LockedBy set<uuid>    // zbiór identyfikatorów automatów, które zajęły dane zlecenie
PRIMARY KEY (StockSymbol, OrderId)
```

### Wykonane transakcje

``` csharp
TransactionId uuid    // identyfikator transakcji
StockSymbol text      // symbol spółki giełowej, której transakcja dotyczy
BuyerId uuid          // identyfikator kupującego
BuyerName text        // imię i nazwisko kupującego
SellerId uuid         // identyfikator sprzedawcy
SellerName text       // imię i nazwisko sprzedawcy
Quantity int          // liczba akcji, która zostały zakupione/sprzedane
PricePerUnit decimal  // cena, po której akcje zostały zakupione/sprzedane 
Date timestamp        // data i czas przeprowadzenia transakcji
PurchaseOrderId uuid  // identyfikator zlecenia zakupu
SaleOrderId uuid      // identyfikator zlecenia sprzedaży
MatcherId uuid        // identyfikator automatu, który jest utworzył transakcję
PRIMARY KEY (StockSymbol, Date)
```

## Zalety

Według założeń rozwiązanie nie dopuszcza do utworzenia niepoprawnych transakcji.

## Wady

Wykorzystujemy poziom  spójności *QUORUM*, w który wymaga odpowiedzi od wielu replik.
Spowalnia to działanie i ryzykuje paraliż systemu, jeśli zbyt wiele replik ulegnie awarii.
