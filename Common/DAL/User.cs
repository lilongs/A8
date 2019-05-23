using Common.DBUtility;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DAL
{
    public class User
    {
        public ObjectId _id;//BsonType.ObjectId 这个对应了 MongoDB.Bson.ObjectId  　　　　
        public string username { get; set; }
        public string password { set; get; }
        public Test test { get; set; }

        public void InsertUser()
        {
            MongoDbCsharpHelper mongoDbHelper = new MongoDbCsharpHelper("mongodb://127.0.0.1:27017", "test");
            User user = new User();
            user.username = "456";
            user.password = "456";
            mongoDbHelper.Insert<User>("user", user);
        }

        public void DeleteUser()
        {
            MongoDbCsharpHelper mongoDbHelper = new MongoDbCsharpHelper("mongodb://127.0.0.1:27017", "test");
            mongoDbHelper.Delete<User>("user", user => user.username == "567");
        }

        public void UpdateUser()
        {
            User user = new User();
            user.username = "567";
            user.password = "567";
            MongoDbCsharpHelper mongoDbHelper = new MongoDbCsharpHelper("mongodb://127.0.0.1:27017", "test");
            mongoDbHelper.Update<User>("user", user, t => t.username == "1234");
        }

        public List<User> SelectUser()
        {
            MongoDbCsharpHelper mongoDbHelper = new MongoDbCsharpHelper("mongodb://127.0.0.1:27017", "test");
            return mongoDbHelper.Find<User>("user",t=>t._id!=null);
        }

        //此方法测试未通过
        public List<User> SelectUserByPage()
        {
            MongoDbCsharpHelper mongoDbHelper = new MongoDbCsharpHelper("mongodb://127.0.0.1:27017", "test");
            return mongoDbHelper.FindByPage<User,User>("user", t => t.username=="123", t => t, 1, 2, out int rsCount);
        }
    }
}
