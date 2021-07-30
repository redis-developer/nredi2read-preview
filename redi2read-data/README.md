# redi2read-data

Sample Data for the Redi2Read Spring Boot / Redis Tutorial

## Books

Data from https://books.googleapis.com using as search criteria CompSci keywords: Redis, Java, Clojure, Spring, etc. to create book objects like:

```
{
  "pageCount":304,
  "thumbnail":"http:\/\/books.google.com\/books\/content?id=prtSDwAAQBAJ&printsec=frontcover&img=1&zoom=1&edge=curl&source=gbs_api",
  "price":42.99,
  "subtitle":null,
  "description":"Drowning in unnecessary complexity, unmanaged state, and tangles of spaghetti code? In the best tradition of Lisp, Clojure gets out of your way so you can focus on expressing simple solutions to hard problems.",
  "language":"en",
  "currency":"USD",
  "id":"1680505726",
  "title":"Programming Clojure",
  "infoLink":"https:\/\/play.google.com\/store\/books\/details?id=prtSDwAAQBAJ&source=gbs_api",
  "authors":[
    "Alex Miller",
    "Stuart Halloway",
    "Aaron Bedra"
  ]
}
```

## Users

Uses https://randomuser.me to create random users like:

```
{
  "password": "9yNvIO4GLBdboI",
  "name": "Georgia Spencer",
  "id": -5035019007718357598,
  "email": "georgia.spencer@example.com"
}
```

