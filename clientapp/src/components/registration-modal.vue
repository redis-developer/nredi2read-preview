<template>
    <b-modal id="registrationModal" ref="registrationModal" hide-footer title="Register" @hidden="onHidden">
    <b-form @submit.prevent="onSubmit" @reset.prevent="onCancel">    
    <b-form-group label="Name:" label-for="name">
        <b-form-input id="name"
            type="text"
            v-model="form.name"
            required
            placeholder="Enter your Name">
        </b-form-input>
    </b-form-group>
      <b-form-group label="Email:" label-for="email">        
        <b-form-input id="email"
                      type="email"
                      v-model="form.email"
                      required
                      placeholder="Enter your email address">
        </b-form-input>        
      </b-form-group>
      <b-form-group label="Password:" label-for="password">
        <b-form-input id="password"
                      type="password"
                      v-model="form.password"
                      required
                      placeholder="Enter your password">
        </b-form-input>
      </b-form-group>
      
      <button class="btn btn-primary float-right ml-2" type="submit">register</button>
      <button class="btn btn-secondary float-right" type="reset">Cancel</button>
    </b-form>
  </b-modal>
</template>

<script>
    import { mapActions } from 'vuex'
    export default{
        data(){
            return{
                form: {
                    email:'',
                    password:'',
                    name:'',
                }
            }
        },
        methods:{
            ...mapActions('context', [
                'register'
            ]),
            onSubmit (evt){
                console.log(evt);
                this.register(this.form).then(res=>{
                    this.$bvModal.hide("registrationModal")
                    alert("Thank you " + res.name + " Account Created!");
                }).catch((err)=>{
                    console.log(err);
                    alert(err.response.data);                                                            
                })
            },
            onCancel (evt){
                console.log(evt);
                this.$refs.registrationModal.hide();                
            },
            onHidden(){
                Object.assign(this.form,{
                    email:'',
                    password:'',
                    name:'',
                })
            }
        }
    }
</script>