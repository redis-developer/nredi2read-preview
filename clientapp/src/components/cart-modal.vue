<template>    
    <b-modal id="cartModal" ref="cartModal" hide-footer title="Cart" @hidden="onHidden">
        <b-form v-if="cart!=null" @submit.prevent="checkout" @reset.prevent="onCancel">
            <ul>
                <li v-for="item in cart.items" v-bind:key="item">
                    <p>ISBN:{{item.isbn}} - price:{{item.price}}</p>
                </li>
            </ul>
            <p>total: {{cartTotal}}</p>
            <button class="btn btn-primary float-right ml-2" type="submit">checkout</button>
        </b-form>        
    </b-modal>
</template>
<script>
import { mapGetters, mapMutations, mapState} from 'vuex'
import axios from 'axios'
export default {
    computed:
    {
        ...mapState('context',
        [
            'cart',
        ]),
        ...mapGetters('context',['cart','cartTotal','profile'])        
    },
    methods:{
        ...mapMutations('context',['setCart']),
        checkout(){            
            // const cartId = this.$store.state.context.cart.id;
            const cartId = this.cart.id
            console.log("checking out " + cartId + " for user: " + this.profile.id);
            axios.post('/api/carts/'+cartId+'/checkout').then(res=>{
                alert(res.data);
                console.log("Creating new cart for "+ this.profile.id)
                axios.post('/api/carts/create?userId=' + this.profile.id).then(res=>{
                    this.setCart(res.data);
                });
            });
            this.$refs.cartModal.hide();
        },
        onCancel(){
            this.$refs.cartModal.hide();
        }        
    },    
}
</script>
