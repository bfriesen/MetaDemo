<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main() // Sneaky Reflection
{
    Customer customer = GetSampleCustomer();
    
    // Our requirements are to send the customer's first, middle, and last names
    // to the InsertCustomer method. But the Customer class doesn't expose its
    // Middle name. We'll need to do some reflection magic here - but let's
    // encapsulate it in the GetMiddleName method.
    
    string middleName = GetMiddleName(customer);
    
    InsertCustomer(customer.FirstName, middleName, customer.LastName);
}

private string GetMiddleName(Customer customer)
{
    string middleName; 
    
    Type customerType = typeof(Customer);
    FieldInfo middleNameField = customerType.GetField("_middleName", BindingFlags.NonPublic | BindingFlags.Instance);
    
    middleName = (string)middleNameField.GetValue(customer);
    
    return middleName;
}

// This is your code. It meets the business requirements of your customer.
public void InsertCustomer(string firstName, string middleName, string lastName)
{
    string.Format("Inserted customer. firstName: {0}, middleName: {1}, lastName: {2}", firstName, middleName, lastName).Dump();
}

// This class is defined in some third-party library that you can't modify.
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

// The means for getting a customer is also defined in some third party assembly that you can't modify
public Customer GetSampleCustomer()
{
    return new Customer("Brian", "Timothy", "Friesen");
}