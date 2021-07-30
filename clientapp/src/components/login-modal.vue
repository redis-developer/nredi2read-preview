<template>
  <b-modal id="loginModal" ref="loginModal" hide-footer title="Login" @hidden="onHidden">
    <b-form @submit.prevent="onSubmit" @reset.prevent="onCancel">      
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
      

      <button class="btn btn-primary float-right ml-2" type="submit">Login</button>
      <button class="btn btn-secondary float-right" type="reset">Cancel</button>
    </b-form>
  </b-modal>
</template>

<script>
import { mapActions } from 'vuex'

export default {
  data () {
    return {
      form: {
        email: '',
        password: ''
      },
      authMode: 'cookie'      
    }
  },
  methods: {
    ...mapActions('context', [
      'login'
    ]),
    onSubmit (evt) {
        console.log(evt);
      this.login({ authMethod: this.authMode, credentials: this.form }).then(() => {
        this.$refs.loginModal.hide()
      }).catch(()=>{
        alert("Email Password combination not valid");        
      })
    },
    onCancel (evt) {
        console.log(evt);
      this.$refs.loginModal.hide()
    },
    onHidden () {
      Object.assign(this.form, {
        email: '',
        password: ''
      })
    }
  }
}
</script>
