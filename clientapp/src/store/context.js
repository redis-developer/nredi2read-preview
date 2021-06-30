import axios from 'axios'
export default {
    namespaced: true,
    state: {
        profile:{},
        cart:{}
    },
    getters: {
        profile: state=>state.profile,
        isAuthenticated: state => state.profile.name && state.profile.email,        
        cart: state=> state.cart,
        cartTotal: state=>{
            if(state.cart!=null && state.cart.items!=null){
                var total = 0
                for(const element of state.cart.items){
                    total+= parseFloat(element.price);
                }
                return total
            }
            else{
                return 0
            }
        }
    },
    mutations: {
        setProfile(state,profile){
            state.profile = profile;
        },
        setCart(state, cart){
            state.cart = cart;
            console.log(cart);
        },
        setNewUserInfo(state,userInfo){
            state.newUserInfo = userInfo;            
        }
    },
    actions: {
        login ({commit}, credentials){
            console.log(credentials.credentials);
            return axios.post('/api/users/login', credentials.credentials).then(res =>{
                commit('setProfile', res.data)
                console.log(res.data);
                return axios.get('api/carts/getByUserId?userId=' + res.data["id"]).then(cart=>{                    
                    commit('setCart',cart.data)
                })
            })
        },
        logout({commit}){
            return axios.post('/api/users/logout').then(()=>{
                commit('setProfile',{})                
            })
        },
        register({commit}, userInfo){
            return axios.post('/api/users/create',userInfo).then(res=>{
                console.log('registered')
                commit('setNewUserInfo',res.data);
                return res.data
            });
        }
    }
  }