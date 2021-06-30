<template>
    <div>
        <div id="details">
            <p class="shortened-details"><b>Title:</b> {{title}}</p>
            <p class="shortened-details"><b>Price:</b> $ {{price}}</p>
            <p class="shortened-details"><b>ISBN:</b> {{isbn}}</p>        
        </div>
        
        <input id="thumbnail" @click="showModal" type="image" :src=thumbnail>
        <br/>
        <button id="addToCartButton" class="btn btn-primary" @click="addToCart">Add to Cart</button>        
        <b-modal :id="'bookModal'+isbn" ref="bookModal" hide-footer>            
            <p><b>Title:</b> {{title}}</p>
            <p><b>Price:</b> $ {{price}}</p>
            <img :src=thumbnail> 
            <p><b>ISBN:</b> {{isbn}}</p>
            <p><b>Description:</b> {{description}}</p>
            <button class="btn btn-primary" @click="addToCart">Add to Cart</button>
        </b-modal>
    </div>    
</template>

<style scoped>
    p.shortened-details {        
        margin-bottom: 1px;
        height: 23px;
        font-size: 18px;
        overflow: hidden;
    }
    #thumbnail{
        max-height: 100px;
        max-width: 100px;
    }    
</style>
<script>
import axios from 'axios'
import { mapGetters, mapMutations} from 'vuex'
export default {
    props:['title','price','thumbnail','isbn', 'description'],
    computed:
    {        
        ...mapGetters('context',['cart',])        
    },
    data () {
        return{
            
        }        
    },
    setup(props) {
        this.title = props.title;
        this.price = parseFloat(props.price).toFixed();
        this.isbn = props.isbn;
        this.thumbnail = props.thumbnail;
        this.description = props.description
    },
    methods:{
        ...mapMutations('context',['setCart']),
        addToCart(){
            const cartId = this.cart.id
            console.log(cartId);
            axios.post("/api/carts/"+cartId+"/addToCart",
                {Isbn:this.isbn, Price:this.price, Quantity:1}).then(res=>{
                    this.setCart(res.data);
                    alert("Added " + this.title + " to cart");
            })
            this.$bvModal.hide('bookModal' + this.isbn)
        },
        showModal(){
            this.$bvModal.show('bookModal' + this.isbn)
        }
    }    
}
</script>
