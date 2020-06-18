using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Codereview
{
    // should not be putting all classes in a single file, lack of proper structure and layering
    // not following proper naming conventions and DDD concepts
    // not sure why the column width is so short causing unnecessary new lines everywhere, I think 120 is now generally the accepted length
    // not using dependency injection which limits composition, substitution at runtime, testing capability and dissallows lifecycle management of dependencies
    // this class does not represent a business feature or domain concept, if we were to take this approach then every domain object would end up having its own manager
    // I would much rather follow the supple design pattern described by Eric Evans in his Domain Driven Design Book, in that design you would have an Application Sevice which would
    // orchestrate between different dependencies in order to complete a feature, for example OrdersService. many developers are familiar are with the supple patterns and new developers on the team would be able to
    // understand and follow the programming style with minimal help which would be another benefit.
       
    public class OrderManager
    {
        private readonly IOrderStore orderStore;
        public OrderManager(IOrderStore orderStore)
        {
            this.orderStore = orderStore;
        }

        // this is not a good name unfortunately, write out to where? it really doesn't describe a domain concept or business functionality, 
        // would be very hard to map the codebase to what business people are trying to achieve and usually talk about in meetings,
        // which would mean debugging and fixing issues or adding new feature would take a longer time because the code does not infer those concepts and topics.
        // unfortunately since our dependencies the store and writer have void return types we are limited to that rather than TPL task type, 
        // which can easily increase the number of the requests that the server can manage while we make blocking calls, 
        // we could wrap the calls to the dependencies in our own Tasks and await them but that will add more complexity, 
        // if we can change the dependencies to return Tasks then that would be my preferred approach
        public void WriteOutSmallOrders()
        {
            var orders = orderStore.GetOrders();

            // this is an external dependency and it is an abstraction which has its own interface, this should be injected through the constructor so that if needed it can be replaced or switched at run time and also
            // to allow for mocking or stubbing during unit tests and integration tests, injecting these dependencies through the constructor also allows you to control their lifecycles independently from the orderservice class
            // this abstraction also breaks the single responsibility principle in SOLID as explained by Robert Martin. the abstraction is supposed to provide different filtration algorithms, 
            // but for some reason it is taking an orderWriter as a dependency to Write the Orders 
            // the Orders Service should be responsible for orchestrating the different calls to the filter and order writer.
            SmallOrderFilter filter = new SmallOrderFilter(new OrderWriter(), orders);

            // from the name of this method it is obvious that it breaks the SRP principle and again it is taking the OrderWriter through method injection but this dependency was already injected through the constructor
            // on the other hand the orders are being injected at initialization through the constructor but not passed through the method arguments, 
            // this demonstrates temporal coupling, meaning that if you were to get a new set of orders between the calls
            // and try to order that new set, you will have to initialize another filter for it to work correctly, 
            // because the filter works on what has been injected through the constructor and does not take the items through method arguments
            filter.WriteOutFiltrdAndPriceSortedOrders(new OrderWriter());
        }

        // the same comments made above in the WriteOutSmallOrders method applies here
        public void WriteOutLargeOrders()
        {
            var orders = orderStore.GetOrders();
            LargeOrderFilter filter = new LargeOrderFilter(new OrderWriter(), orders);
            filter.WriteOutFiltrdAndPriceSortedOrders(new OrderWriter());
        }
    }
    
    
    // this class is overengineering a trivial problem and adding and unnecessary amount of complexity
    // breaks KISS and YAGNI principles, takes DRY too far
    // we simply want to filter orders, but this calls is creating abstractions and subclassing 
    // subclassing has less benefits than composition using interfaces as the base class properties and behavior will be harder to change and replace during runtime
    // in order to allow for flexibility this is made even more complex by breaking encapsulation and making behaviour and properties of this class visible to subclasses using the protected access identifier
    public class LargeOrderFilter
    {
        // this should ideally be taken out and injected to the order service as this classes responsibility is to do filteration,
        // apart from that dependencies be marked readonly so that they can't be changed after initialization
        private IOrderWriter orderWriter;

        // this should be a method parameter so that this filteration logic can be used for different sets of collections that will be passed as method argument
        private List<Order> orders;

        public LargeOrderFilter(IOrderWriter orderWriter, List<Order> orders)
        {
            filterSize = "100";
            this.orderWriter = orderWriter;
            this.orders = orders;
        }

        // this variable can be easily passed a method argument and this class or abstraction could easily be renamed or changed to OrderSizeFilter
        // this class should not be used as a base class, this could easily break the Liskov Substitution Principle if the classes is inherited in the wrong context
        // having a string to represent a number is a bad idea, it requires you to do casts and conversions and does not allow you to fail fast which means having to diagnose issues at run time rather than compile time
        protected string filterSize;

        
        public void WriteOutFiltrdAndPriceSortedOrders(IOrderWriter writer)
        {
            
            // but here we are filtering a list and returning a new one, and this operation will have to go through all items so the reactive property of being able to show the items on the screen one by one as they get filtered is diminished
            // on the other hand since we are filtering items we dont really have to enumerate through all the items, we could have used IEnumberable which is a lazy pull based collection and would only return an item if asked for it
            // which would mean we wouldn't unnecessarily be enumerating the whole collection and would allow us to chain operations together
            List<Order> filteredOrders = this.FilterOrdersSmallerThan(orders,
            filterSize);

            // this operation is lost as the return type is not taken into account
            // also it unnecessarily allocates memory as the operation returns a new collection 
            Enumerable.OrderBy(filteredOrders, o => o.Price);
            
            // not sure if this is necessarily the right collection to use as the OrderWriter seems to be taking an IEnumerable
            // also it unnecessarily allocates memory, it could have easily just forwarded the list as it implements IEnumerable
            // observable collection is a push based collection meant for implementing reactive applications, allowing to react to events as soon as they happen, 
            // for this reason it is best to chain these collections together, but here it wont work properly as the OrderWriter is taking an IEnumerable
            // also it could have easily used var on the left side as it is easily visible on the right side what the var is being assigned to
            ObservableCollection<Order> observableCollection =
            new ObservableCollection<Order>();

            // as mentioned above the reactive property is diminished as at this point all calculations have already been done and all orders have to be available before being pushed to the observable collection
            foreach (Order o in filteredOrders)
            {
                observableCollection.Add(o);
            }
            
            // another reason why use of ObservableCollection is unnecessary is that OrderWriter takes an IEnumerable
            // unless order writer is casting the IEnumerable to an ObservableCollection which is a terrible idea
            writer.WriteOrders(observableCollection);
        }

        // this could easily be implemented using a where clause in LINQ and should not really be made to be overriden 
        // protected without the virtual keyword is also going to require the use of the new keyword
        protected List<Order> FilterOrdersSmallerThan(List<Order> allOrders,
        string size)
        {
            //use var as it is obviouse what type is being used on the right side
            List<Order> filtered = new List<Order>();

            //this <= will cause an index out of range exception
            for (int i = 0; i <= allOrders.Count; i++)
            {
                // this would throw if size is not a valid int32, not sure why another object is responsible for converting a string to an int
                // totally unnecessary no need for this string to be converted in each iteration of the loop, in fact it should have been an int from the start
                // number is not a good name for a variable, size isnt either tbh
                // size should have been converted outside the loop
                int number = orders[i].toNumber(size);
                if (allOrders[i].Size <= number)
                {
                    continue;
                }
                else
                {
                    filtered.Add(orders[i]);
                }
            }
            return filtered;
        }
    }
    
    // always prefer composition over inheritance
    // this breaks the Liskov Substitution Principle, and from an object oriended design point of view and 
    // Conceptually does not make sense, how can an SmallFilter alos be a LargeFilter?"
    // totally unnecessary and overengineered abstraction and subclassing 
    // the little amount of overriden code in the constructor is a hint, 
    // suffers from all the issues above as it inherits that code as its base class
    public class SmallOrderFilter : LargeOrderFilter
    {
        public SmallOrderFilter(IOrderWriter orderWriter, List<Order> orders)
        : base(orderWriter, orders)
        {
            //really bad to be using string to represent an int
            filterSize = "10";
        }
    }
    // this class is being used in different layers, layers should pass data through their own dedicated DTOs and Contracts, 
    // its generally bad practice to use your Data Model throughout all layers as changes will be cascaded through everything and your level control over those potential changes will be diminished
    // if this is a DTO that's being returned by the storage or an api, then I expect all these values to be valid and therefore all the properties should be changed to be auto properties
    // if this was a domain model then I would see benefit in encapsulation, backing fields for properties and validation etc
    public class Order
    {
        // can be changed to an autoproperty
        public double Price
        {
            get { return this.dPrice; }
            set { this.dPrice = value; }
        }

        // can be changed to an autoproperty
        public int Size
        {
            get { return this.iSize; }
            set { this.iSize = value; }
        }

        // can be changed to an autoproperty
        public string Symbol
        {
            get { return this.sSymbol; }
            set { this.sSymbol = value; }
        }

        // unnecessary set of private variables with incorrect naming convention
        private double dPrice;
        private int iSize;
        private string sSymbol;


        // misplaced method which does not provide any extra benefit and overcomplicates a simple conversion operation
        // method argument should be changed to use lowercase string type alias as per general Microsoft guidelines
        // method names generally start with Uppercase letter
        public int toNumber(String Input)
        {

            //default values are already false and 0;
            bool canBeConverted = false;
            int n = 0;
            try
            {
                //could get rid of try catch and use int.TryParse, significantly reducing number of lines and complexity
                n = Convert.ToInt32(Input);
                if (n != 0) canBeConverted = true;
            }
            // swallowing all types of exceptions rather than handling specific ones
            catch (Exception ex)
            {
                //could simply return the default n here
            }
            //no need for these two blocks if returning n early
            if (canBeConverted == true)
            {
                return n;
            }
            else
            {
                return 0;
            }
        }
    }
    // These are stub interfaces that already exist in the system
    // They're out of scope of the code review
    public interface IOrderWriter
    {
        void WriteOrders(IEnumerable<Order> orders);
    }
    public class OrderWriter : IOrderWriter
    {
        public void WriteOrders(IEnumerable<Order> orders)
        {
        }
    }
    public interface IOrderStore
    {
        List<Order> GetOrders();
    }
}
