//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SwaggerSharp2
//{

//	public class Rootobject
//	{
//		public string swagger { get; set; }
//		public Info info { get; set; }
//		public string host { get; set; }
//		public string basePath { get; set; }
//		public Tag1[] tags { get; set; }
//		public string[] schemes { get; set; }
//		public Paths paths { get; set; }
//		public Securitydefinitions securityDefinitions { get; set; }
//		public Definitions definitions { get; set; }
//		public Externaldocs externalDocs { get; set; }
//	}

//	public class Info
//	{
//		public string description { get; set; }
//		public string version { get; set; }
//		public string title { get; set; }
//		public string termsOfService { get; set; }
//		public Contact contact { get; set; }
//		public License license { get; set; }
//	}

//	public class Contact
//	{
//		public string email { get; set; }
//	}

//	public class License
//	{
//		public string name { get; set; }
//		public string url { get; set; }
//	}

//	public class Paths
//	{
//		public Pet pet { get; set; }
//		public PetFindbystatus petfindByStatus { get; set; }
//		public PetFindbytags petfindByTags { get; set; }
//		public PetPetid petpetId { get; set; }
//		public PetPetidUploadimage petpetIduploadImage { get; set; }
//		public StoreInventory storeinventory { get; set; }
//		public StoreOrder storeorder { get; set; }
//		public StoreOrderOrderid storeorderorderId { get; set; }
//		public User user { get; set; }
//		public UserCreatewitharray usercreateWithArray { get; set; }
//		public UserCreatewithlist usercreateWithList { get; set; }
//		public UserLogin userlogin { get; set; }
//		public UserLogout userlogout { get; set; }
//		public UserUsername userusername { get; set; }
//	}

//	public class Pet
//	{
//		public Post post { get; set; }
//		public Put put { get; set; }
//	}

//	public class Post
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] consumes { get; set; }
//		public string[] produces { get; set; }
//		public Parameter[] parameters { get; set; }
//		public Responses responses { get; set; }
//		public Security[] security { get; set; }
//	}

//	public class Responses
//	{
//		public _405 _405 { get; set; }
//	}

//	public class _405
//	{
//		public string description { get; set; }
//	}

//	public class Parameter
//	{
//		public string _in { get; set; }
//		public string name { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public Schema schema { get; set; }
//	}

//	public class Schema
//	{
//		public string _ref { get; set; }
//	}

//	public class Security
//	{
//		public string[] petstore_auth { get; set; }
//	}

//	public class Put
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] consumes { get; set; }
//		public string[] produces { get; set; }
//		public Parameter1[] parameters { get; set; }
//		public Responses1 responses { get; set; }
//		public Security1[] security { get; set; }
//	}

//	public class Responses1
//	{
//		public _400 _400 { get; set; }
//		public _404 _404 { get; set; }
//		public _4051 _405 { get; set; }
//	}

//	public class _400
//	{
//		public string description { get; set; }
//	}

//	public class _404
//	{
//		public string description { get; set; }
//	}

//	public class _4051
//	{
//		public string description { get; set; }
//	}

//	public class Parameter1
//	{
//		public string _in { get; set; }
//		public string name { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public Schema1 schema { get; set; }
//	}

//	public class Schema1
//	{
//		public string _ref { get; set; }
//	}

//	public class Security1
//	{
//		public string[] petstore_auth { get; set; }
//	}

//	public class PetFindbystatus
//	{
//		public Get get { get; set; }
//	}

//	public class Get
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter2[] parameters { get; set; }
//		public Responses2 responses { get; set; }
//		public Security2[] security { get; set; }
//	}

//	public class Responses2
//	{
//		public _200 _200 { get; set; }
//		public _4001 _400 { get; set; }
//	}

//	public class _200
//	{
//		public string description { get; set; }
//		public Schema2 schema { get; set; }
//	}

//	public class Schema2
//	{
//		public string type { get; set; }
//		public Items items { get; set; }
//	}

//	public class Items
//	{
//		public string _ref { get; set; }
//	}

//	public class _4001
//	{
//		public string description { get; set; }
//	}

//	public class Parameter2
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//		public Items1 items { get; set; }
//		public string collectionFormat { get; set; }
//	}

//	public class Items1
//	{
//		public string type { get; set; }
//		public string[] _enum { get; set; }
//		public string _default { get; set; }
//	}

//	public class Security2
//	{
//		public string[] petstore_auth { get; set; }
//	}

//	public class PetFindbytags
//	{
//		public Get1 get { get; set; }
//	}

//	public class Get1
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter3[] parameters { get; set; }
//		public Responses3 responses { get; set; }
//		public Security3[] security { get; set; }
//	}

//	public class Responses3
//	{
//		public _2001 _200 { get; set; }
//		public _4002 _400 { get; set; }
//	}

//	public class _2001
//	{
//		public string description { get; set; }
//		public Schema3 schema { get; set; }
//	}

//	public class Schema3
//	{
//		public string type { get; set; }
//		public Items2 items { get; set; }
//	}

//	public class Items2
//	{
//		public string _ref { get; set; }
//	}

//	public class _4002
//	{
//		public string description { get; set; }
//	}

//	public class Parameter3
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//		public Items3 items { get; set; }
//		public string collectionFormat { get; set; }
//	}

//	public class Items3
//	{
//		public string type { get; set; }
//	}

//	public class Security3
//	{
//		public string[] petstore_auth { get; set; }
//	}

//	public class PetPetid
//	{
//		public Get2 get { get; set; }
//		public Post1 post { get; set; }
//		public Delete delete { get; set; }
//	}

//	public class Get2
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter4[] parameters { get; set; }
//		public Responses4 responses { get; set; }
//		public Security4[] security { get; set; }
//	}

//	public class Responses4
//	{
//		public _2002 _200 { get; set; }
//		public _4003 _400 { get; set; }
//		public _4041 _404 { get; set; }
//	}

//	public class _2002
//	{
//		public string description { get; set; }
//		public Schema4 schema { get; set; }
//	}

//	public class Schema4
//	{
//		public string _ref { get; set; }
//	}

//	public class _4003
//	{
//		public string description { get; set; }
//	}

//	public class _4041
//	{
//		public string description { get; set; }
//	}

//	public class Parameter4
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Security4
//	{
//		public object[] api_key { get; set; }
//	}

//	public class Post1
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] consumes { get; set; }
//		public string[] produces { get; set; }
//		public Parameter5[] parameters { get; set; }
//		public Responses5 responses { get; set; }
//		public Security5[] security { get; set; }
//	}

//	public class Responses5
//	{
//		public _4052 _405 { get; set; }
//	}

//	public class _4052
//	{
//		public string description { get; set; }
//	}

//	public class Parameter5
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Security5
//	{
//		public string[] petstore_auth { get; set; }
//	}

//	public class Delete
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter6[] parameters { get; set; }
//		public Responses6 responses { get; set; }
//		public Security6[] security { get; set; }
//	}

//	public class Responses6
//	{
//		public _4004 _400 { get; set; }
//	}

//	public class _4004
//	{
//		public string description { get; set; }
//	}

//	public class Parameter6
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//		public string description { get; set; }
//		public string format { get; set; }
//	}

//	public class Security6
//	{
//		public string[] petstore_auth { get; set; }
//	}

//	public class PetPetidUploadimage
//	{
//		public Post2 post { get; set; }
//	}

//	public class Post2
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] consumes { get; set; }
//		public string[] produces { get; set; }
//		public Parameter7[] parameters { get; set; }
//		public Responses7 responses { get; set; }
//		public Security7[] security { get; set; }
//	}

//	public class Responses7
//	{
//		public _2003 _200 { get; set; }
//	}

//	public class _2003
//	{
//		public string description { get; set; }
//		public Schema5 schema { get; set; }
//	}

//	public class Schema5
//	{
//		public string _ref { get; set; }
//	}

//	public class Parameter7
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Security7
//	{
//		public string[] petstore_auth { get; set; }
//	}

//	public class StoreInventory
//	{
//		public Get3 get { get; set; }
//	}

//	public class Get3
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public object[] parameters { get; set; }
//		public Responses8 responses { get; set; }
//		public Security8[] security { get; set; }
//	}

//	public class Responses8
//	{
//		public _2004 _200 { get; set; }
//	}

//	public class _2004
//	{
//		public string description { get; set; }
//		public Schema6 schema { get; set; }
//	}

//	public class Schema6
//	{
//		public string type { get; set; }
//		public Additionalproperties additionalProperties { get; set; }
//	}

//	public class Additionalproperties
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Security8
//	{
//		public object[] api_key { get; set; }
//	}

//	public class StoreOrder
//	{
//		public Post3 post { get; set; }
//	}

//	public class Post3
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter8[] parameters { get; set; }
//		public Responses9 responses { get; set; }
//	}

//	public class Responses9
//	{
//		public _2005 _200 { get; set; }
//		public _4005 _400 { get; set; }
//	}

//	public class _2005
//	{
//		public string description { get; set; }
//		public Schema7 schema { get; set; }
//	}

//	public class Schema7
//	{
//		public string _ref { get; set; }
//	}

//	public class _4005
//	{
//		public string description { get; set; }
//	}

//	public class Parameter8
//	{
//		public string _in { get; set; }
//		public string name { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public Schema8 schema { get; set; }
//	}

//	public class Schema8
//	{
//		public string _ref { get; set; }
//	}

//	public class StoreOrderOrderid
//	{
//		public Get4 get { get; set; }
//		public Delete1 delete { get; set; }
//	}

//	public class Get4
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter9[] parameters { get; set; }
//		public Responses10 responses { get; set; }
//	}

//	public class Responses10
//	{
//		public _2006 _200 { get; set; }
//		public _4006 _400 { get; set; }
//		public _4042 _404 { get; set; }
//	}

//	public class _2006
//	{
//		public string description { get; set; }
//		public Schema9 schema { get; set; }
//	}

//	public class Schema9
//	{
//		public string _ref { get; set; }
//	}

//	public class _4006
//	{
//		public string description { get; set; }
//	}

//	public class _4042
//	{
//		public string description { get; set; }
//	}

//	public class Parameter9
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//		public float maximum { get; set; }
//		public float minimum { get; set; }
//		public string format { get; set; }
//	}

//	public class Delete1
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter10[] parameters { get; set; }
//		public Responses11 responses { get; set; }
//	}

//	public class Responses11
//	{
//		public _4007 _400 { get; set; }
//		public _4043 _404 { get; set; }
//	}

//	public class _4007
//	{
//		public string description { get; set; }
//	}

//	public class _4043
//	{
//		public string description { get; set; }
//	}

//	public class Parameter10
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//		public float minimum { get; set; }
//	}

//	public class User
//	{
//		public Post4 post { get; set; }
//	}

//	public class Post4
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter11[] parameters { get; set; }
//		public Responses12 responses { get; set; }
//	}

//	public class Responses12
//	{
//		public Default _default { get; set; }
//	}

//	public class Default
//	{
//		public string description { get; set; }
//	}

//	public class Parameter11
//	{
//		public string _in { get; set; }
//		public string name { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public Schema10 schema { get; set; }
//	}

//	public class Schema10
//	{
//		public string _ref { get; set; }
//	}

//	public class UserCreatewitharray
//	{
//		public Post5 post { get; set; }
//	}

//	public class Post5
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter12[] parameters { get; set; }
//		public Responses13 responses { get; set; }
//	}

//	public class Responses13
//	{
//		public Default1 _default { get; set; }
//	}

//	public class Default1
//	{
//		public string description { get; set; }
//	}

//	public class Parameter12
//	{
//		public string _in { get; set; }
//		public string name { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public Schema11 schema { get; set; }
//	}

//	public class Schema11
//	{
//		public string type { get; set; }
//		public Items4 items { get; set; }
//	}

//	public class Items4
//	{
//		public string _ref { get; set; }
//	}

//	public class UserCreatewithlist
//	{
//		public Post6 post { get; set; }
//	}

//	public class Post6
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter13[] parameters { get; set; }
//		public Responses14 responses { get; set; }
//	}

//	public class Responses14
//	{
//		public Default2 _default { get; set; }
//	}

//	public class Default2
//	{
//		public string description { get; set; }
//	}

//	public class Parameter13
//	{
//		public string _in { get; set; }
//		public string name { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public Schema12 schema { get; set; }
//	}

//	public class Schema12
//	{
//		public string type { get; set; }
//		public Items5 items { get; set; }
//	}

//	public class Items5
//	{
//		public string _ref { get; set; }
//	}

//	public class UserLogin
//	{
//		public Get5 get { get; set; }
//	}

//	public class Get5
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter14[] parameters { get; set; }
//		public Responses15 responses { get; set; }
//	}

//	public class Responses15
//	{
//		public _2007 _200 { get; set; }
//		public _4008 _400 { get; set; }
//	}

//	public class _2007
//	{
//		public string description { get; set; }
//		public Schema13 schema { get; set; }
//		public Headers headers { get; set; }
//	}

//	public class Schema13
//	{
//		public string type { get; set; }
//	}

//	public class Headers
//	{
//		public XRateLimit XRateLimit { get; set; }
//		public XExpiresAfter XExpiresAfter { get; set; }
//	}

//	public class XRateLimit
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//		public string description { get; set; }
//	}

//	public class XExpiresAfter
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//		public string description { get; set; }
//	}

//	public class _4008
//	{
//		public string description { get; set; }
//	}

//	public class Parameter14
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//	}

//	public class UserLogout
//	{
//		public Get6 get { get; set; }
//	}

//	public class Get6
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public object[] parameters { get; set; }
//		public Responses16 responses { get; set; }
//	}

//	public class Responses16
//	{
//		public Default3 _default { get; set; }
//	}

//	public class Default3
//	{
//		public string description { get; set; }
//	}

//	public class UserUsername
//	{
//		public Get7 get { get; set; }
//		public Put1 put { get; set; }
//		public Delete2 delete { get; set; }
//	}

//	public class Get7
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter15[] parameters { get; set; }
//		public Responses17 responses { get; set; }
//	}

//	public class Responses17
//	{
//		public _2008 _200 { get; set; }
//		public _4009 _400 { get; set; }
//		public _4044 _404 { get; set; }
//	}

//	public class _2008
//	{
//		public string description { get; set; }
//		public Schema14 schema { get; set; }
//	}

//	public class Schema14
//	{
//		public string _ref { get; set; }
//	}

//	public class _4009
//	{
//		public string description { get; set; }
//	}

//	public class _4044
//	{
//		public string description { get; set; }
//	}

//	public class Parameter15
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//	}

//	public class Put1
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter16[] parameters { get; set; }
//		public Responses18 responses { get; set; }
//	}

//	public class Responses18
//	{
//		public _40010 _400 { get; set; }
//		public _4045 _404 { get; set; }
//	}

//	public class _40010
//	{
//		public string description { get; set; }
//	}

//	public class _4045
//	{
//		public string description { get; set; }
//	}

//	public class Parameter16
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//		public Schema15 schema { get; set; }
//	}

//	public class Schema15
//	{
//		public string _ref { get; set; }
//	}

//	public class Delete2
//	{
//		public string[] tags { get; set; }
//		public string summary { get; set; }
//		public string description { get; set; }
//		public string operationId { get; set; }
//		public string[] produces { get; set; }
//		public Parameter17[] parameters { get; set; }
//		public Responses19 responses { get; set; }
//	}

//	public class Responses19
//	{
//		public _40011 _400 { get; set; }
//		public _4046 _404 { get; set; }
//	}

//	public class _40011
//	{
//		public string description { get; set; }
//	}

//	public class _4046
//	{
//		public string description { get; set; }
//	}

//	public class Parameter17
//	{
//		public string name { get; set; }
//		public string _in { get; set; }
//		public string description { get; set; }
//		public bool required { get; set; }
//		public string type { get; set; }
//	}

//	public class Securitydefinitions
//	{
//		public Petstore_Auth petstore_auth { get; set; }
//		public Api_Key api_key { get; set; }
//	}

//	public class Petstore_Auth
//	{
//		public string type { get; set; }
//		public string authorizationUrl { get; set; }
//		public string flow { get; set; }
//		public Scopes scopes { get; set; }
//	}

//	public class Scopes
//	{
//		public string writepets { get; set; }
//		public string readpets { get; set; }
//	}

//	public class Api_Key
//	{
//		public string type { get; set; }
//		public string name { get; set; }
//		public string _in { get; set; }
//	}

//	public class Definitions
//	{
//		public Order Order { get; set; }
//		public Category Category { get; set; }
//		public User User { get; set; }
//		public Tag Tag { get; set; }
//		public Apiresponse ApiResponse { get; set; }
//		public Pet Pet { get; set; }
//	}

//	public class Order
//	{
//		public string type { get; set; }
//		public Properties properties { get; set; }
//		public Xml xml { get; set; }
//	}

//	public class Properties
//	{
//		public Id id { get; set; }
//		public Petid petId { get; set; }
//		public Quantity quantity { get; set; }
//		public Shipdate shipDate { get; set; }
//		public Status status { get; set; }
//		public Complete complete { get; set; }
//	}

//	public class Id
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Petid
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Quantity
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Shipdate
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Status
//	{
//		public string type { get; set; }
//		public string description { get; set; }
//		public string[] _enum { get; set; }
//	}

//	public class Complete
//	{
//		public string type { get; set; }
//		public bool _default { get; set; }
//	}

//	public class Xml
//	{
//		public string name { get; set; }
//	}

//	public class Category
//	{
//		public string type { get; set; }
//		public Properties1 properties { get; set; }
//		public Xml1 xml { get; set; }
//	}

//	public class Properties1
//	{
//		public Id1 id { get; set; }
//		public Name name { get; set; }
//	}

//	public class Id1
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Name
//	{
//		public string type { get; set; }
//	}

//	public class Xml1
//	{
//		public string name { get; set; }
//	}

//	public class User
//	{
//		public string type { get; set; }
//		public Properties2 properties { get; set; }
//		public Xml2 xml { get; set; }
//	}

//	public class Properties2
//	{
//		public Id2 id { get; set; }
//		public Username username { get; set; }
//		public Firstname firstName { get; set; }
//		public Lastname lastName { get; set; }
//		public Email email { get; set; }
//		public Password password { get; set; }
//		public Phone phone { get; set; }
//		public Userstatus userStatus { get; set; }
//	}

//	public class Id2
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Username
//	{
//		public string type { get; set; }
//	}

//	public class Firstname
//	{
//		public string type { get; set; }
//	}

//	public class Lastname
//	{
//		public string type { get; set; }
//	}

//	public class Email
//	{
//		public string type { get; set; }
//	}

//	public class Password
//	{
//		public string type { get; set; }
//	}

//	public class Phone
//	{
//		public string type { get; set; }
//	}

//	public class Userstatus
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//		public string description { get; set; }
//	}

//	public class Xml2
//	{
//		public string name { get; set; }
//	}

//	public class Tag
//	{
//		public string type { get; set; }
//		public Properties3 properties { get; set; }
//		public Xml3 xml { get; set; }
//	}

//	public class Properties3
//	{
//		public Id3 id { get; set; }
//		public Name1 name { get; set; }
//	}

//	public class Id3
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Name1
//	{
//		public string type { get; set; }
//	}

//	public class Xml3
//	{
//		public string name { get; set; }
//	}

//	public class Apiresponse
//	{
//		public string type { get; set; }
//		public Properties4 properties { get; set; }
//	}

//	public class Properties4
//	{
//		public Code code { get; set; }
//		public Type type { get; set; }
//		public Message message { get; set; }
//	}

//	public class Code
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Type
//	{
//		public string type { get; set; }
//	}

//	public class Message
//	{
//		public string type { get; set; }
//	}

//	public class Pet
//	{
//		public string type { get; set; }
//		public string[] required { get; set; }
//		public Properties5 properties { get; set; }
//		public Xml6 xml { get; set; }
//	}

//	public class Properties5
//	{
//		public Id4 id { get; set; }
//		public Category1 category { get; set; }
//		public Name2 name { get; set; }
//		public Photourls photoUrls { get; set; }
//		public Tags tags { get; set; }
//		public Status1 status { get; set; }
//	}

//	public class Id4
//	{
//		public string type { get; set; }
//		public string format { get; set; }
//	}

//	public class Category1
//	{
//		public string _ref { get; set; }
//	}

//	public class Name2
//	{
//		public string type { get; set; }
//		public string example { get; set; }
//	}

//	public class Photourls
//	{
//		public string type { get; set; }
//		public Xml4 xml { get; set; }
//		public Items6 items { get; set; }
//	}

//	public class Xml4
//	{
//		public string name { get; set; }
//		public bool wrapped { get; set; }
//	}

//	public class Items6
//	{
//		public string type { get; set; }
//	}

//	public class Tags
//	{
//		public string type { get; set; }
//		public Xml5 xml { get; set; }
//		public Items7 items { get; set; }
//	}

//	public class Xml5
//	{
//		public string name { get; set; }
//		public bool wrapped { get; set; }
//	}

//	public class Items7
//	{
//		public string _ref { get; set; }
//	}

//	public class Status1
//	{
//		public string type { get; set; }
//		public string description { get; set; }
//		public string[] _enum { get; set; }
//	}

//	public class Xml6
//	{
//		public string name { get; set; }
//	}

//	public class Externaldocs
//	{
//		public string description { get; set; }
//		public string url { get; set; }
//	}

//	public class Tag1
//	{
//		public string name { get; set; }
//		public string description { get; set; }
//		public Externaldocs1 externalDocs { get; set; }
//	}

//	public class Externaldocs1
//	{
//		public string description { get; set; }
//		public string url { get; set; }
//	}

//}
