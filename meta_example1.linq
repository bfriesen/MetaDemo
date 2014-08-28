<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#define NONEST
void Main()
{
    var customer = GetCustomer();
    
    // Fine then. We'll do it the hard way.
    var middleNameField = typeof(Customer).GetField("_middleName", BindingFlags.Instance | BindingFlags.NonPublic);
    
    InsertCustomer(customer.FirstName, (string)middleNameField.GetValue(customer), customer.LastName);
}

// This is your code. It meets the business requirements of your customer.
public void InsertCustomer(string firstName, string middleName, string lastName)
{
    string.Format("Inserted customer. firstName: {0}, middleName: {1}, lastName: {2}", firstName, middleName, lastName).Dump();
}

// The means for getting a customer is also defined in some third party assembly that you can't modify
public Customer GetCustomer()
{
    return new Customer("Brian", "Timothy", "Friesen");
}

// This class is defined in some third-party assembly that you can't modify.
public class Customer
{
    private readonly string _firstName;
    private readonly string _middleName;
    private readonly string _lastName;

    public Customer(string firstName, string middleName, string lastName)
    {
        _firstName = firstName;
        _middleName = middleName;
        _lastName = lastName;
    }
    
    public string FirstName
    {
        get { return _firstName; }
    }
    
    public string LastName
    {
        get { return _lastName; }
    }
    
    public string FullName
    {
        get
        {
            if (string.IsNullOrEmpty(_middleName))
            {
                return _firstName + " " + _lastName;
            }
            
            return _firstName + " " + _middleName[0] + ". " + _lastName;
        }
    }
}