# AbstractXServer #

You should override only one abstract method in your server implementaion:
  * [IStorage](IStorage.md) CreateStorage()

Now X-Transport has only one class that implements interface [IStorage](IStorage.md). I is [SQLiteStorage](SQLiteStorage.md)