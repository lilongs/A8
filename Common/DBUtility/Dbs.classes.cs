﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace Common.DBUtility
{          
    public class TempTest : DbsBaseClass
    {
        public TempTest()
        {
            connectionString = GetconnectionString();
        }
    }    

    public class DbSQL
    {
        public string SQL;
        public SqlParameter[] Params;
    }
}
